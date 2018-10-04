using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace DistributieTESTWebServices
{
    public class OperatiiSoferi
    {

        private static readonly int MEDIA_KM_ZI = 750;

        public string getSoferi()
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<Sofer> listSoferi = new List<Sofer>();
            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select a.fili, upper(a.nume), b.codTableta  from soferi a, sapprd.ztabletesoferi b where a.cod = b.codsofer " +
                                  " and b.stare = 1 order by a.fili,a.nume ";

                cmd.Parameters.Clear();

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        Sofer sofer = new Sofer();
                        sofer.filiala = oReader.GetString(0);
                        sofer.nume = oReader.GetString(1);
                        sofer.codTableta = oReader.GetString(2);
                        listSoferi.Add(sofer);

                    }
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return new JavaScriptSerializer().Serialize(listSoferi);

        }



        public static bool isSoferDTI(OracleConnection conn, String codSofer)
        {

            bool isDTI = false;

            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            cmd = conn.CreateCommand();

            cmd.CommandText = " select fili from soferi where cod =:codSofer ";

            cmd.Parameters.Clear();
            cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
            cmd.Parameters[0].Value = codSofer;

            oReader = cmd.ExecuteReader();

            oReader.Read();

            if (oReader.GetString(0).Equals("GL90"))
                isDTI = true;

            DatabaseConnections.CloseConnections(oReader, cmd);

            return isDTI;
        }




        public string verificaKmSalvati(string nrAuto, string kmNoi)
        {

            int kmSalvati = 0;
            string dataKmSalvati = "";
            string msgStatus = "";
            int msgId = 0;
            string numeSofer = "";

            bool isKmSalvatiValid = true;

            StareValidareKm stareValidare = new StareValidareKm();

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                connection.ConnectionString = DatabaseConnections.ConnectToTestEnvironment();
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select to_char(to_date(k.datac,'yyyymmdd')), k.km, s.nume from sapprd.zkmmasini k, soferi s where k.nrauto =:nrAuto " +
                                  " and k.datac = (select max(datac) from sapprd.zkmmasini where nrauto = k.nrAuto) and s.cod = k.codangajat ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 30).Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters[0].Value = nrAuto;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    kmSalvati = oReader.GetInt32(1);
                    dataKmSalvati = oReader.GetString(0);
                    numeSofer = oReader.GetString(2);

                    if (Int32.Parse(kmNoi) <= kmSalvati)
                    {
                        msgStatus = "Acest index este mai mic decat cel salvat anterior. In data de " + formatDate(dataKmSalvati) + ", " + numeSofer + " a introdus indexul " +kmSalvati + ".";
                        isKmSalvatiValid = false;
                        msgId = 1;
                    }
                    else if (Int32.Parse(kmNoi) > kmSalvati)
                    {
                        string dataStart = dataKmSalvati;
                        string dataStop = getCurrentDateFormatted();

                        DateTime dateStart = Convert.ToDateTime(dataStart);
                        DateTime dateStop = Convert.ToDateTime(dataStop);

                        TimeSpan ts = dateStop - dateStart;

                        int mediaKm = 1;

                        if (ts.Days > 0)
                            mediaKm = (Int32.Parse(kmNoi) - kmSalvati) / (ts.Days);

                        if (mediaKm > MEDIA_KM_ZI)
                        {
                            msgStatus = "Au fost parcursi mai mult de " + MEDIA_KM_ZI + " km/zi.";
                            isKmSalvatiValid = false;
                            msgId = 2;
                        }

                    }


                    if (isKmSalvatiValid)
                    {

                        cmd.CommandText = " select min(mileage) km, 'astazi' timp from gps_date where " +
                                          " device_id = (select id from gps_masini where nr_masina =:nrAuto) and trunc(record_time) = trunc(sysdate) " +
                                          " union " +
                                          " select min(mileage) km, 'trecut' timp from gps_date where " +
                                          " device_id = (select id from gps_masini where nr_masina =:nrAuto)  and trunc(record_time) =:dataSalvare  ";

                        cmd.Parameters.Clear();

                        cmd.Parameters.Add(":nrAuto", OracleType.VarChar, 10).Direction = System.Data.ParameterDirection.Input;
                        cmd.Parameters[0].Value = nrAuto.Replace("-", "").Replace(" ", ""); 

                        cmd.Parameters.Add(":dataSalvare", OracleType.VarChar, 10).Direction = System.Data.ParameterDirection.Input;
                        cmd.Parameters[1].Value = dataKmSalvati;

                        oReader = cmd.ExecuteReader();

                        int kmGpsAstazi = 0;
                        int kmGpsTrecut = 0;

                        if (oReader.HasRows)
                        {
                            while (oReader.Read())
                            {
                                if (oReader.GetString(1).Equals("astazi"))
                                    kmGpsAstazi = oReader.GetInt32(0);

                                if (oReader.GetString(1).Equals("trecut"))
                                    kmGpsTrecut = oReader.GetInt32(0);
                            }

                        }

                        double distEfectuataGps = kmGpsAstazi - kmGpsTrecut;
                        double distEfectuataDeclarat = Int32.Parse(kmNoi) - kmSalvati;

                        double difKmDistanta = Math.Abs(distEfectuataDeclarat - distEfectuataGps);

                        bool isDifProcent = Math.Abs(distEfectuataGps - distEfectuataDeclarat) > distEfectuataGps / 10;

                        if (difKmDistanta > 10 || isDifProcent)
                        {
                            msgStatus = "De la ultima salvare, conform GPS, masina a parcurs " + distEfectuataGps + " km, dar conform km bord introdusi sunt " + distEfectuataDeclarat + " km. Confirmati km din bord?";
                            isKmSalvatiValid = false;
                            msgId = 3;
                        }



                    }


                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            stareValidare.isKmValid = isKmSalvatiValid;
            stareValidare.statusMsg = msgStatus;
            stareValidare.statusId = msgId;

            return new JavaScriptSerializer().Serialize(stareValidare);
        }


        private string getCurrentDateFormatted()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString("0000");
            string day = cDate.Day.ToString("00");
            string month = cDate.ToString("MMM");
            mDate = day + "-" + month + "-" + year;
            return mDate;
        }



        private string formatDate(string strDate)
        {
            string[] tokDate = strDate.Split('-');
            return tokDate[0] + "-" + tokDate[1] + "-20" + tokDate[2];


        }


        public string getMasinaSofer(string codSofer)
        {
            string nrMasina = "";

            nrMasina = getMasinaSoferBorderou(codSofer);

            if (nrMasina.Length == 0)
                nrMasina = getMasinaSoferAlocata(codSofer);

            if (nrMasina.Length == 0)
                nrMasina = " ";

            return nrMasina;
        }


        public string getMasinaSoferBorderou(string codSofer)
        {
            string nrMasina = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select x.masina from (select to_char(b.data_e),  nvl((select nvl(ev.eveniment,'0') eveniment " +
                                  " from sapprd.zevenimentsofer ev where ev.document = b.numarb and ev.data = (select max(data) from sapprd.zevenimentsofer where document = ev.document and client = ev.document) " +
                                  " and ev.ora = (select max(ora) from sapprd.zevenimentsofer where document = ev.document and client = ev.document and data = ev.data)),0) eveniment, b.shtyp, " +
                                  " nvl((select distinct nr_bord from sapprd.zdocumentebord where nr_bord_urm =b.numarb and rownum=1),'-1') bordParent, b.masina " +
                                  " from  borderouri b, sapprd.zdocumentebord c  where  c.nr_bord = b.numarb and b.cod_sofer=:codSofer  order by trunc(c.data_e), c.ora_e) x where x.eveniment != 'S' and rownum<2 ";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    nrMasina = oReader.GetString(0);

                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }


            return nrMasina;
        }


        public string getMasinaSoferAlocata(string codSofer)
        {
            string nrMasina = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select ktext from( select distinct c.ktext,a.adatu " +
                                  " from sapprd.anlz a join sapprd.anla b on b.anln1 = a.anln1 and b.anln2 = a.anln2 and b.mandt = a.mandt " +
                                  " join sapprd.aufk c on c.aufnr = a.caufn and c.mandt = a.mandt where a.pernr = :codSofer " +
                                  " and a.bdatu >= (select to_char(sysdate - 5, 'YYYYMMDD') from dual) and b.deakt = '00000000' and a.mandt = '900' and c.auart = '1001' " +
                                  " order by a.adatu desc ) where rownum = 1 ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    nrMasina = oReader.GetString(0);
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return nrMasina;
        }

        public string getKmMasina(string nrMasina)
        {
            string kmMasina = "0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select x.mileage from( select mileage from gps_date where " +
                                  " device_id = (select id from gps_masini where nr_masina =:nrMasina) " +
                                  " and trunc(record_time) = trunc(sysdate) order by mileage) x where rownum = 1 ";

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrMasina", OracleType.VarChar, 10).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrMasina.Replace("-", "").Replace(" ", "");

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    kmMasina = oReader.GetInt32(0).ToString();
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd, connection);
            }

            return kmMasina;



        }

    }
}