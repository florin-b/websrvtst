using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Web.Script.Serialization;
using System.Globalization;

namespace DistributieTESTWebServices
{
    public class OperatiiEvenimente
    {
        public string saveNewEvent(string serializedEvent)
        {


            string retVal = "";

            if (serializedEvent.Contains("["))
                saveEventsList(serializedEvent);
            else
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                EvenimentNou newEvent = serializer.Deserialize<EvenimentNou>(serializedEvent);


                OracleConnection connection = new OracleConnection();
                OracleDataReader oReader = null;
                string query = "";

                DateTime cDate = DateTime.Now;
                string year = cDate.Year.ToString();
                string day = cDate.Day.ToString("00");
                string month = cDate.Month.ToString("00");
                string nowDate = year + month + day;

                string hour = cDate.Hour.ToString("00");
                string minute = cDate.Minute.ToString("00");
                string sec = cDate.Second.ToString("00");
                string nowTime = hour + minute + sec;


                string strLocalEveniment = "0";

                //eveniment document
                if (newEvent.document.Equals(newEvent.client))
                {
                    if (newEvent.eveniment.Equals("0"))
                        strLocalEveniment = "P";

                    if (newEvent.eveniment.Equals("P"))
                        strLocalEveniment = "S";
                }
                else //eveniment client
                {
                    if (newEvent.eveniment.Equals("0") || newEvent.eveniment.Equals("P"))
                        strLocalEveniment = "S";

                    if (newEvent.eveniment.Equals("S"))
                        strLocalEveniment = "P";
                }


                try
                {

                    string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                    connection.ConnectionString = connectionString;
                    connection.Open();

                    OracleCommand cmd = connection.CreateCommand();

                    if (newEvent.tipEveniment == null || newEvent.tipEveniment.Equals("NOU", StringComparison.InvariantCultureIgnoreCase))
                    {
                        query = " select d.latitude, d.longitude, nvl(d.mileage,0) from gps_index d where d.device_id = (select distinct g.id from sapprd.zdocumentesms a, " +
                                " borderouri b, gps_masini g where  " +
                                " a.nr_bord = b.numarb and REPLACE(b.masina, '-') = g.nr_masina " +
                                " and a.nr_bord = :nrBord)  ";
                    }
                    else if (newEvent.tipEveniment.Equals("ARHIVAT", StringComparison.InvariantCultureIgnoreCase))
                    {
                        string dataEveniment = newEvent.data + " " + newEvent.ora;

                        nowDate = newEvent.data;
                        nowTime = newEvent.ora;

                        query = " select latitude, longitude, mileage from ( " +
                                " select latitude, longitude, mileage, " +
                                " (to_date('" + dataEveniment + "', 'yyyymmdd hh24miss') - record_time) * 24 * 60  diff from gps_date where device_id = " +
                                " (select id from gps_masini where nr_masina = (select replace(masina, '-', '') from websap.borderouri where numarb =:nrBord)) and " +
                                " record_time between(to_date('" + dataEveniment + "', 'yyyymmdd hh24miss') - 15 / (24 * 60)) and(to_date('" + dataEveniment + "', 'yyyymmdd hh24miss') + 15 / (24 * 60)) " +
                                " and(to_date('" + dataEveniment + "', 'yyyymmdd hh24miss') - record_time) * 24 * 60 >= 0 order by diff ) where rownum< 2 ";
                    }

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":nrBord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = newEvent.document;

                    cmd.CommandText = query;

                    oReader = cmd.ExecuteReader();
                    string latit = "0", longit = "0", mileage = "0";

                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        latit = oReader.GetDouble(0).ToString().Replace(",", ".");
                        longit = oReader.GetDouble(1).ToString().Replace(",", ".");
                        mileage = oReader.GetDecimal(2).ToString();
                    }


                    if (recordExist(connection, newEvent.codSofer, newEvent.document, newEvent.client, strLocalEveniment, newEvent.codAdresa))
                    {
                        retVal = nowDate + "#" + nowTime;
                        return retVal;
                    }


                    query = " insert into sapprd.zevenimentsofer(mandt,codsofer,data,ora,document,client,eveniment,gps,fms,codadresa) " +
                            " values ('900',:codsofer,:data,:ora,:document,:client,:eveniment,:gps,:fms,:codadresa) ";


                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = newEvent.codSofer;

                    cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = nowDate;

                    cmd.Parameters.Add(":ora", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = nowTime;

                    cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = newEvent.document;

                    cmd.Parameters.Add(":client", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = newEvent.client;

                    cmd.Parameters.Add(":eveniment", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                    cmd.Parameters[5].Value = strLocalEveniment;

                    cmd.Parameters.Add(":gps", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                    cmd.Parameters[6].Value = latit + "," + longit;

                    cmd.Parameters.Add(":fms", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                    cmd.Parameters[7].Value = mileage;

                    cmd.Parameters.Add(":codadresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[8].Value = newEvent.codAdresa;

                    cmd.ExecuteNonQuery();

                    retVal = nowDate + "#" + nowTime;


                    if (!newEvent.bordParent.Equals("-1"))
                        saveBordParentSfCursa(connection, newEvent, latit, longit, mileage);


                    if (cmd != null)
                        cmd.Dispose();


                }
                catch (Exception ex)
                {
                    retVal = nowDate + "#" + nowTime;
                    ErrorHandling.sendErrorToMail(ex.ToString() + " data = " + newEvent.truckData);
                }
                finally
                {
                    connection.Close();
                    connection.Dispose();
                }
            }


            return retVal;
        }




        private bool recordExist(OracleConnection connection, string codSofer, string document, string codClient, string eveniment, string codAdresa)
        {
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            bool exists = false;

            try
            {
                string sqlString = " select 1 from sapprd.zevenimentsofer where codsofer=:codSofer and document=:document and client=:codclient and eveniment=:eveniment  " +
                                   " and codadresa=:codAdresa ";

                cmd = connection.CreateCommand();
                cmd.CommandText = sqlString;

                cmd.CommandType = CommandType.Text;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = document;

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codClient;

                cmd.Parameters.Add(":eveniment", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = eveniment;

                cmd.Parameters.Add(":codadresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = codAdresa;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    exists = true;
                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                if (oReader != null)
                    oReader.Close();

                if (cmd != null)
                    cmd.Dispose();

            }



            return exists;


        }



        private void saveEventsList(string serializedEvents)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<EvenimentNou> evenimente = serializer.Deserialize<List<EvenimentNou>>(serializedEvents);

            for (int i = 0; i < evenimente.Count; i++)
                saveNewEvent(serializer.Serialize(evenimente[i]));


        }




        private void saveBordParentSfCursa(OracleConnection connection, EvenimentNou newEvent, String latit, String longit, String mileage)
        {



            try
            {

                OracleCommand cmd = connection.CreateCommand();

                String query = " insert into sapprd.zevenimentsofer(mandt,codsofer,data,ora,document,client,eveniment,gps,fms,codadresa) " +
                               " values ('900',:codsofer,:data,:ora,:document,:client,:eveniment,:gps,:fms,:codadresa) ";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = newEvent.codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = getDate();

                cmd.Parameters.Add(":ora", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = getTime();

                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = newEvent.document;

                cmd.Parameters.Add(":client", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = newEvent.document;

                cmd.Parameters.Add(":eveniment", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = "S";

                cmd.Parameters.Add(":gps", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = latit + "," + longit;

                cmd.Parameters.Add(":fms", OracleType.VarChar, 600).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = mileage;

                cmd.Parameters.Add(":codadresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[8].Value = newEvent.codAdresa;


                cmd.ExecuteNonQuery();

                if (cmd != null)
                    cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {

            }



        }



        public string saveEvenimentStopIncarcare(string document, string codSofer)
        {

            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                String query = " insert into sapprd.zsfarsitinc(mandt, document, codsofer, data, ora) " +
                               " values ('900', :document, :codsofer, :data, :ora) ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = document;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":data", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = getDate();

                cmd.Parameters.Add(":ora", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = getTime();

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                return "-1";
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return "SOF";

        }



        public void saveOrdineEtape(string serializedEtape)
        {

            OracleConnection connection = new OracleConnection();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            List<Etapa> listEtape = serializer.Deserialize<List<Etapa>>(serializedEtape);

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                if (ordineEtapeExista(connection, listEtape[0].borderou))
                {
                    connection.Close();
                    connection.Dispose();
                    return;
                }

                OracleCommand cmd = connection.CreateCommand();


                foreach (Etapa etapa in listEtape)
                {
                    String query = " insert into sapprd.zordinelivrari(mandt, borderou, client, codadresa, pozitie, document) " +
                                   " values ('900', :boderou, :client, :codAdresa, :pozitie, :document) ";

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":boderou", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = etapa.borderou;

                    cmd.Parameters.Add(":client", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[1].Value = etapa.client;

                    cmd.Parameters.Add(":codAdresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[2].Value = etapa.codAdresa;

                    cmd.Parameters.Add(":pozitie", OracleType.Int32, 10).Direction = ParameterDirection.Input;
                    cmd.Parameters[3].Value = etapa.pozitie;

                    cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                    cmd.Parameters[4].Value = etapa.document;

                    cmd.ExecuteNonQuery();
                }


                cmd.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());

            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }

            return;

        }



        private bool ordineEtapeExista(OracleConnection connection, string bord)
        {
            OracleCommand cmd = connection.CreateCommand();
            OracleDataReader oReader = null;

            bool etapeExists = false;
            try
            {

                cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapprd.zordinelivrari where borderou=:bord  ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":bord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = bord;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                    etapeExists = true;

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {

                if (oReader != null)
                    oReader.Close();

                if (cmd != null)
                    cmd.Dispose();

            }

            return etapeExists;

        }


        public string getEvenimentStopIncarcare(string document, string codSofer)
        {
            string retValue = " ";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select 1 from sapprd.zsfarsitinc where document=:document and codsofer =:codSofer ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = document;

                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    retValue = "SOF";
                }
                else
                {
                    cmd.CommandText = " select dalen from sapprd.vttk where tknum=:document ";

                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":document", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = document;

                    oReader = cmd.ExecuteReader();

                    if (oReader.HasRows)
                    {
                        oReader.Read();

                        if (oReader.GetString(0) == "00000000")
                            retValue = " ";
                        else
                            retValue = "LOG";



                    }




                }

                oReader.Close();
                oReader.Dispose();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
                return "-1";
            }
            finally
            {
                cmd.Dispose();
                connection.Close();
                connection.Dispose();
            }




            return retValue;


        }



        public string saveNewStop(string idEveniment, string codSofer, string codBorderou, string codEveniment)
        {

            OracleConnection connection = new OracleConnection();

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                String query = " insert into sapprd.zopririsoferi(mandt, ideveniment, codsofer, codborderou, codeveniment, data, ora) " +
                               " values ('900', :ideveniment, :codsofer, :codborderou, :codeveniment, :data, :ora) ";


                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":ideveniment", OracleType.VarChar, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = idEveniment;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":codborderou", OracleType.VarChar, 15).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codBorderou;

                cmd.Parameters.Add(":codeveniment", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = codEveniment;

                cmd.Parameters.Add(":data", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = getDate();

                cmd.Parameters.Add(":ora", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = getTime();

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            return "1";
        }

        public string getEvenimenteBorderou(string nrBorderou)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select  b.nume,b.cod , " +
                                  " nvl((select  c.data||':'||c.ora||'?'|| c.fms from sapprd.zevenimentsofer c where c.document = a.nr_bord  and c.document = c.client and  " +
                                  " c.eveniment = 'P'),0) plecare_cursa, " +
                                  " nvl((select  c.data||':'||c.ora||'?'|| c.fms from sapprd.zevenimentsofer c where c.document = a.nr_bord and c.client = a.cod_client and " +
                                  " c.eveniment = 'S' and c.codadresa = a.adresa_client),0) sosire, nvl((select c.ora from sapprd.zevenimentsofer c where c.document = a.nr_bord and c.client = a.cod_client " +
                                  " and c.eveniment = 'P' and c.codadresa = a.adresa_client),0) plecare, a.adresa_client cod_adresa, " +
                                  " (select ad.city1||', '||ad.street||', '||ad.house_num1 from sapprd.adrc ad where ad.client = '900' and ad.addrnumber = a.adresa_client) adresa_client " +
                                  " from sapprd.zdocumentesms a, clienti b  where a.nr_bord =:nrbord " +
                                  " and a.cod_client = b.cod and tip = 2 order by a.poz ";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrbord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBorderou;

                oReader = cmd.ExecuteReader();
                string[] startCursa, sosireClient;
                string oraStartCursa = "0", kmStartCursa = "0";
                string oraSosireClient = "0", kmSosireClient = "0";

                List<EvenimentBorderou> listEvenimente = new List<EvenimentBorderou>();
                EvenimentBorderou eveniment = null;


                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        oraStartCursa = "0"; kmStartCursa = "0";
                        oraSosireClient = "0"; kmSosireClient = "0";

                        if (!oReader.GetString(2).Equals("0"))
                        {
                            startCursa = oReader.GetString(2).Split('?');
                            oraStartCursa = startCursa[0];
                            kmStartCursa = startCursa[1];
                        }

                        if (!oReader.GetString(3).Equals("0"))
                        {
                            sosireClient = oReader.GetString(3).Split('?');
                            oraSosireClient = sosireClient[0];
                            kmSosireClient = sosireClient[1];
                        }

                        eveniment = new EvenimentBorderou();
                        eveniment.numeClient = oReader.GetString(0);
                        eveniment.codClient = oReader.GetString(1);
                        eveniment.oraStartCursa = oraStartCursa;
                        eveniment.kmStartCursa = kmStartCursa;
                        eveniment.oraSosireClient = oraSosireClient;
                        eveniment.kmSosireClient = kmSosireClient;
                        eveniment.oraPlecare = oReader.GetString(4);
                        eveniment.codAdresa = oReader.GetString(5);
                        eveniment.adresa = oReader.GetString(6);
                        listEvenimente.Add(eveniment);

                    }

                }


                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listEvenimente);

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


            return serializedResult;
        }


        public string getEvenimenteBorderouri(string codSofer, string interval)
        {
            string condData = "";
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {

                if (interval == "0") //astazi
                {
                    string dateInterval = DateTime.Today.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    condData = " and s.data_e = '" + dateInterval + "' ";
                }

                if (interval == "1") //ultimele 7 zile
                {
                    string dateInterval = DateTime.Today.AddDays(-7).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    condData = " and s.data_e >= '" + dateInterval + "' ";
                }

                if (interval == "2") //ultimele 30 zile
                {
                    string dateInterval = DateTime.Today.AddDays(-30).ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    condData = " and s.data_e >= '" + dateInterval + "' ";
                }


                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select distinct b.document,  to_char(to_date(s.data_e,'yyyyMMdd')),  nvl((select nvl(ev.eveniment,'0') eveniment " +
                                  " from sapprd.zevenimentsofer ev where ev.document = b.document and ev.data = (select max(data) from sapprd.zevenimentsofer where document = ev.document and client = ev.document) " +
                                  " and ev.ora = (select max(ora) from sapprd.zevenimentsofer where document = ev.document and client = ev.document and data = ev.data)),0) eveniment " +
                                  " from  sapprd.zevenimentsofer b, sapprd.zdocumentesms s where b.codsofer=:codSofer and s.nr_bord = b.document " + condData + " order by b.document ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;


                oReader = cmd.ExecuteReader();

                List<Borderouri> listaBorderouri = new List<Borderouri>();
                Borderouri unBorderou = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        unBorderou = new Borderouri();
                        unBorderou.numarBorderou = oReader.GetString(0);
                        unBorderou.dataEmiterii = oReader.GetString(1);
                        unBorderou.evenimentBorderou = oReader.GetString(2);
                        listaBorderouri.Add(unBorderou);


                    }

                }


                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaBorderouri);

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


            return serializedResult;
        }



        public static InitStatus getInitStatus(string codSofer)
        {
            InitStatus initStatus = null;

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select ev.document, ev.client, ev.eveniment, b.shtyp from sapprd.zevenimentsofer ev, borderouri b where " +
                                  " b.numarb = ev.document and ev.codsofer =:codSofer and " +
                                  " ev.data = (select max(ev1.data) from sapprd.zevenimentsofer ev1 where ev1.codsofer = ev.codsofer) and " +
                                  " ev.ora = (select max(ev2.ora) from sapprd.zevenimentsofer ev2 where ev2.codsofer = ev.codsofer and ev2.data = ev.data) and " +
                                  " exists (select 1 from sapprd.zevenimentsofer z where z.codsofer = ev.codsofer and z.client = ev.document " +
                                  " and z.data = ev.data and z.ora = z.ora and z.eveniment = 'P') " +
                                  " and not exists (select 1 from sapprd.zevenimentsofer z where z.codsofer = ev.codsofer and z.client = ev.document " +
                                  " and z.data = ev.data and z.ora = z.ora and z.eveniment = 'S') ";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    initStatus = new InitStatus();
                    initStatus.document = oReader.GetString(0);
                    initStatus.client = oReader.GetString(1);
                    initStatus.eveniment = oReader.GetString(2);
                    initStatus.tipDocument = oReader.GetString(3);
                }

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

            return initStatus;
        }


        public string cancelEvent(string tipEveniment, string nrDocument, string codClient, string codSofer)
        {
            string retVal = "";


            if (tipEveniment.Equals("SFARSIT_INCARCARE"))
            {
                retVal = cancelSfarsitIncarcare(nrDocument, codSofer);
            }
            else if (tipEveniment.Equals("START_BORD") || tipEveniment.Equals("STOP_BORD"))
            {
                retVal = cancelStartBord(nrDocument, codSofer, tipEveniment);
            }
            else if (tipEveniment.Equals("SOSIRE"))
            {
                retVal = cancelSosire(nrDocument, codClient, codSofer);
            }



            return retVal;
        }



        public string getPozitieCurenta(string nrBorderou)
        {

            string retVal = "0x0";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();


                cmd.CommandText = " select nvl(latitude,0), nvl(longitude,0) from gps_index where device_id = ( " +
                                  " select id from gps_masini where nr_masina = " +
                                  " (select replace(masina, '-', '') from borderouri where numarb =:nrBorderou)) ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrBorderou", OracleType.VarChar, 300).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBorderou;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    retVal = oReader.GetDouble(0) + "x" + oReader.GetDouble(1);

                }

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



            retVal = "0x0";
            return retVal;
        }


        private string cancelSfarsitIncarcare(string nrDocument, string codSofer)
        {
            OracleConnection connection = new OracleConnection();
            string retVal = "1";



            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                String query = " delete from sapprd.zsfarsitinc where document=:nrDocument and codsofer=:codSofer ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDocument", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            return retVal;



        }



        private string cancelStartBord(string nrDocument, string codSofer, string tipEveniment)
        {
            OracleConnection connection = new OracleConnection();
            string retVal = "1";


            string strEveniment = "X";
            if (tipEveniment.Equals("START_BORD"))
                strEveniment = "P";
            else if (tipEveniment.Equals("STOP_BORD"))
                strEveniment = "S";



            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                String query = " delete from sapprd.zevenimentsofer where document=:nrDocument and client=:nrDocument and codsofer=:codSofer and eveniment =:eveniment ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDocument", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codSofer;

                cmd.Parameters.Add(":eveniment", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = strEveniment;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            return retVal;



        }



        private string cancelSosire(string nrDocument, string codClient, string codSofer)
        {
            OracleConnection connection = new OracleConnection();
            string retVal = "1";



            try
            {
                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                String query = " delete from sapprd.zevenimentsofer where document=:nrDocument and client=:codClient and codsofer=:codSofer and eveniment = 'S' ";

                cmd.CommandType = CommandType.Text;
                cmd.CommandText = query;
                cmd.Parameters.Clear();

                cmd.Parameters.Add(":nrDocument", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDocument;

                cmd.Parameters.Add(":codClient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;

                cmd.Parameters.Add(":codsofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codSofer;

                cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }


            return retVal;



        }




        private static String getDate()
        {
            DateTime cDate = DateTime.Now;
            string year = cDate.Year.ToString();
            string day = cDate.Day.ToString("00");
            string month = cDate.Month.ToString("00");
            return year + month + day;
        }

        private static String getTime()
        {
            DateTime cDate = DateTime.Now;
            string hour = cDate.Hour.ToString("00");
            string minute = cDate.Minute.ToString("00");
            string sec = cDate.Second.ToString("00");
            return hour + minute + sec;
        }


    }
}