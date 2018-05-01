using System;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;

namespace DistributieTESTWebServices
{
    public class OperatiiMasina
    {
        private static readonly int MEDIA_KM_ZI = 750;

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

                connection.ConnectionString = DatabaseConnections.ConnectToProdEnvironment();
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

                    if (Int32.Parse(kmNoi) < kmSalvati)
                    {
                        msgStatus = "Acest index este mai mic decat cel salvat anterior. In data de " + formatDate(dataKmSalvati) + ", " + numeSofer + " a introdus indexul " + kmSalvati + ".";
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
                            msgStatus = "Conform acestui index au fost parcursi mai mult de " + MEDIA_KM_ZI + " km/zi.";
                            isKmSalvatiValid = false;
                            msgId = 2;
                        }

                    }


                    if (isKmSalvatiValid)
                    {

                        cmd.CommandText = " select nvl(min(mileage),0) km, 'astazi' timp from gps_date where " +
                                          " device_id = (select id from gps_masini where nr_masina =:nrAuto) and trunc(record_time) = trunc(sysdate) " +
                                          " union " +
                                          " select nvl(min(mileage),0) km, 'trecut' timp from gps_date where " +
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
                            msgStatus = "De la ultima salvare, facuta in data de " + formatDate(dataKmSalvati) + ", conform GPS, masina a parcurs " + distEfectuataGps + " km, dar conform km bord introdusi sunt " + distEfectuataDeclarat + " km. Confirmati km din bord?";
                            isKmSalvatiValid = false;
                            msgId = 3;
                        }



                    }


                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString() + " , nrauto " + nrAuto + " , kmNoi " + kmNoi);
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



        private string getCurrentDate()
        {
            string mDate = "";
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            mDate = year + month + day;
            return mDate;
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


    }
}