using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.OracleClient;
using System.Data.Common;
using System.Data;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;


namespace DistributieTESTWebServices
{
    public class OperatiiDocumente
    {

        public string getDocEvents(string nrDoc, string tipEv)
        {
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            string tipEveniment = "";

            if (tipEv.Equals("0"))
            {
                tipEveniment = " and ev.eveniment in ('0','P','S') ";
            }


            try
            {

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

                cmd.CommandText = " select  ev.eveniment, to_char(to_date(ev.data,'yyyyMMdd')) data_ev, ev.ora, ev.fms from sapprd.zevenimentsofer ev where document = client and " +
                                  " ev.document =:document " + tipEveniment + " order by ev.eveniment, ev.data, ev.ora ";

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":document", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrDoc;

                oReader = cmd.ExecuteReader();

                List<Eveniment> listEvenimente = new List<Eveniment>();
                Eveniment eveniment = null;


                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        eveniment = new Eveniment();
                        eveniment.eveniment = oReader.GetString(0);
                        eveniment.data = oReader.GetString(1);
                        eveniment.ora = oReader.GetString(2);
                        eveniment.distantaKM = oReader.GetString(3);
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


        public string getBorderouri(string codSofer, string interval, string tip)
        {
            string condData = "", condTip = "";
            string serializedResult = "";

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;
            List<Borderouri> listaBorderouri = new List<Borderouri>();

            try
            {

                if (interval == "0") //astazi
                {
                    string dateInterval = DateTime.Today.ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    condData = " and b.data_e = '" + dateInterval + "' ";
                }

                if (interval == "1") //ultimele 7 zile
                {
                    string dateInterval = DateTime.Today.AddDays(-7).ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    condData = " and b.data_e >= '" + dateInterval + "' ";
                }

                if (interval == "2") //ultimele 30 zile
                {
                    string dateInterval = DateTime.Today.AddDays(-30).ToString("dd-MMM-yyyy", System.Globalization.CultureInfo.InvariantCulture);
                    condData = " and b.data_e >= '" + dateInterval + "' ";
                }

                condData = "";

                condTip = "";
                if (tip.Equals("d"))    //doar borderourile deschise
                {
                    condTip = " where x.eveniment != 'S' and rownum<2 ";
                }


                string connectionString = DatabaseConnections.ConnectToTestEnvironment();

                connection.ConnectionString = connectionString;
                connection.Open();

                cmd = connection.CreateCommand();

              



                cmd.CommandText = " select x.* from (select b.numarb,  to_char(b.data_e),  nvl((select nvl(ev.eveniment,'0') eveniment " +
                                  " from sapprd.zevenimentsofer ev where ev.document = b.numarb and ev.data = (select max(data) from sapprd.zevenimentsofer where document = ev.document and client = ev.document) " +
                                  " and ev.ora = (select max(ora) from sapprd.zevenimentsofer where document = ev.document and client = ev.document and data = ev.data)),0) eveniment, b.shtyp, " +
                                  " nvl((select distinct nr_bord from sapprd.zdocumentebord where nr_bord_urm =b.numarb),'-1') bordParent " +
                                  " from  borderouri b, sapprd.zdocumentebord c  where  c.nr_bord = b.numarb and b.cod_sofer=:codSofer " + condData + " order by trunc(c.data_e), c.ora_e) x " + condTip;

                
               

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codSofer", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codSofer;

                oReader = cmd.ExecuteReader();

               
                Borderouri unBorderou = null;

                if (oReader.HasRows)
                {

                    while (oReader.Read())
                    {
                        unBorderou = new Borderouri();
                        unBorderou.numarBorderou = oReader.GetString(0);
                        unBorderou.dataEmiterii = oReader.GetString(1);
                        unBorderou.evenimentBorderou = oReader.GetString(2);
                        unBorderou.tipBorderou = oReader.GetString(3);
                        unBorderou.bordParent = oReader.GetString(4);
                        listaBorderouri.Add(unBorderou);

                    }

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


            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializedResult = serializer.Serialize(listaBorderouri);

            return serializedResult;
        }




        public string getArticoleBorderou(string nrBorderou, string codClient, string codAdresa)
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

                cmd.CommandText = " select a.textmat, a.cantitate, a.umcant, 'descarcare' tipOp, a.spart, a.greutbruta, a.umgreut from sapprd.zcom a, sapprd.zcomm ad, " +
                                  " sapprd.zdl_inc c, sapprd.zdocumentesms b where a.nrcom = ad.nrcom and a.docn = ad.docn and a.docp = ad.docp and a.etenr = ad.etenr " +
                                  " and b.adresa_client = ad.adrnz and b.nr_bord =:nrbord  and c.nrcom  = b.nrct and c.nrcominc = a.nrcom  and a.primitor =:codclient " +
                                  " and b.adresa_client =:codadresa " +
                                  " union " +
                                  " select a.textmat,a.cantitate, a.umcant, 'incarcare' tipOp, a.spart, a.greutbruta, a.umgreut from sapprd.zcom a, sapprd.zcomm ad, " +
                                  " sapprd.zdl_inc c, sapprd.zdocumentesms b where a.nrcom = ad.nrcom and a.docn = ad.docn and a.docp = ad.docp and a.etenr = ad.etenr " +
                                  " and b.adresa_furnizor = ad.adrna and b.nr_bord =:nrbord and c.nrcom  = b.nrct and c.nrcominc = a.nrcom  and predator =:codclient " +
                                  " and b.adresa_furnizor =:codadresa " +
                                  " union " +
                                  " select a.textmat, a.cantitate, a.umcant,'descarcare' tipOp, a.spart, a.greutbruta, a.umgreut from sapprd.zcom a, sapprd.zcomm ad, " +
                                  " sapprd.zdocumentesms b where a.nrcom = ad.nrcom and a.docn = ad.docn and a.docp = ad.docp and a.etenr = ad.etenr and " +
                                  " b.adresa_client = ad.adrnz and b.nr_bord =:nrbord  and b.nrct  = a.nrcom and a.primitor =:codclient and b.adresa_client =:codadresa " +
                                  " union " +
                                  " select a.textmat, a.cantitate, a.umcant, 'incarcare' tipOp, a.spart, a.greutbruta, a.umgreut from sapprd.zcom a,sapprd.zcomm ad, " +
                                  " sapprd.zdocumentesms b where a.nrcom = ad.nrcom and a.docn = ad.docn and a.docp = ad.docp and a.etenr = ad.etenr and " +
                                  " b.adresa_furnizor = ad.adrna and b.nr_bord =:nrbord and b.nrct = a.nrcom  and predator =:codclient and " +
                                  " b.adresa_furnizor =:codadresa order by tipOp, textmat ";



                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrbord", OracleType.VarChar, 36).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBorderou;

                cmd.Parameters.Add(":codclient", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codClient;

                cmd.Parameters.Add(":codadresa", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = codAdresa;

                oReader = cmd.ExecuteReader();

                List<ArticoleFactura> listArticole = new List<ArticoleFactura>();
                ArticoleFactura unArticol = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        unArticol = new ArticoleFactura();
                        unArticol.nume = Regex.Replace(oReader.GetString(0), @"[!]|[#]|[@@]|[,]", " ");
                        unArticol.cantitate = oReader.GetDouble(1).ToString();
                        unArticol.umCant = oReader.GetString(2);
                        unArticol.tipOperatiune = oReader.GetString(3);
                        unArticol.departament = oReader.GetString(4);
                        unArticol.greutate = oReader.GetDouble(5).ToString();
                        unArticol.umGreutate = oReader.GetString(6);
                        listArticole.Add(unArticol);
                    }

                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listArticole);

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



        public string getFacturiBorderou(string nrBorderou, string tipBorderou)
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

                //start cursa
                cmd.CommandText = " select  c.data||':'||c.ora from sapprd.zevenimentsofer c where c.document =:nrBord and c.document = c.client and  c.eveniment = 'P' ";
                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrbord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBorderou;

                oReader = cmd.ExecuteReader();
                string startCursa = "";
                if (oReader.HasRows)
                {
                    oReader.Read();
                    startCursa = oReader.GetString(0);
                }




                if (tipBorderou.ToLower().Equals("distributie"))
                {
                    cmd.CommandText = " select  a.nume, b.cod , nvl((select c.ora from sapprd.zevenimentsofer c where c.document = a.nr_bord and c.client = a.cod and " +
                                      " c.eveniment = 'S' and c.codadresa = a.adresa),0) sosire, nvl((select c.ora from sapprd.zevenimentsofer c where c.document = a.nr_bord and c.client = a.cod " +
                                      " and c.eveniment = 'P' and c.codadresa = a.adresa),0) plecare, " +
                                      " nvl((select ad.region||','||ad.city1||', '||ad.street||', '||ad.house_num1 from sapprd.adrc ad where ad.client = '900' and ad.addrnumber = a.adresa),' ') adresa_client, " +
                                      " a.adresa, " +
                                      " nvl((select pozitie from sapprd.zordinelivrari where borderou = a.nr_bord and client = a.cod and codadresa = a.adresa ),'-1') pozitie, a.nr_bord" + 
                                      " from sapprd.zdocumentebord a, clienti b  where a.nr_bord =:nrbord " +
                                      " and a.cod = b.cod order by a.poz ";
                }

                if (tipBorderou.ToLower().Equals("aprovizionare") || tipBorderou.ToLower().Equals("inchiriere") || tipBorderou.ToLower().Equals("service"))
                {
                    cmd.CommandText = " select a.nume, a.cod, " +
                                      " (select ad.region||','||ad.city1||', '||ad.street||', '||ad.house_num1 from sapprd.adrc ad where ad.client = '900' and ad.addrnumber = a.adresa) adresa, " +
                                      " nvl((select c.ora from sapprd.zevenimentsofer c where c.document = a.nr_bord and c.client = a.cod and " +
                                      " c.eveniment = 'S' and c.codadresa = a.adresa),0) sosire_furnizor, nvl((select c.ora from sapprd.zevenimentsofer c where c.document = a.nr_bord and c.client = a.cod " +
                                      " and c.eveniment = 'P' and c.codadresa = a.adresa),0) plecare_furnizor, a.nume, a.cod , " +
                                      " (select ad.region||','||ad.city1||', '||ad.street||', '||ad.house_num1 from sapprd.adrc ad where ad.client = '900' and ad.addrnumber = a.adresa) adresa_client, " +
                                      " nvl((select c.ora from sapprd.zevenimentsofer c where c.document = a.nr_bord and c.client = a.cod and " +
                                      " c.eveniment = 'S' and c.codadresa = a.adresa),0) sosire_client, nvl((select c.ora from sapprd.zevenimentsofer c where c.document = a.nr_bord and c.client = a.cod " +
                                      " and c.eveniment = 'P' and c.codadresa = a.adresa),0) plecare_client, a.adresa, a.adresa from sapprd.zdocumentebord a where a.nr_bord =:nrbord " +
                                      "  order by a.poz";
                }


               
                

                cmd.Parameters.Clear();
                cmd.Parameters.Add(":nrbord", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = nrBorderou;

                oReader = cmd.ExecuteReader();

                List<FacturiBorderou> listaFacturi = new List<FacturiBorderou>();
                FacturiBorderou oFactura = null;

                if (oReader.HasRows)
                {
                    while (oReader.Read())
                    {
                        if (tipBorderou.ToLower().Equals("distributie"))
                        {
                            oFactura = new FacturiBorderou();
                            oFactura.numeClient = oReader.GetString(0);
                            oFactura.codClient = oReader.GetString(1);
                            oFactura.sosireClient = oReader.GetString(2);
                            oFactura.plecareClient = oReader.GetString(3);
                            oFactura.adresaClient = UtilsAddresses.formatAdresa(oReader.GetString(4));
                            oFactura.codAdresaClient = oReader.GetString(5);
                            oFactura.pozitie = oReader.GetInt32(6).ToString();
                            oFactura.nrFactura = oReader.GetString(7).Length > 1 ? oReader.GetString(7) : nrBorderou;
                            oFactura.dataStartCursa = startCursa;

                            oFactura.numeFurnizor = "";
                            oFactura.codFurnizor = "";
                            oFactura.adresaFurnizor = "";
                            oFactura.sosireFurnizor = "";
                            oFactura.plecareFurnizor = "";

                            listaFacturi.Add(oFactura);


                        }

                        if (tipBorderou.ToLower().Equals("aprovizionare") || tipBorderou.ToLower().Equals("inchiriere") || tipBorderou.ToLower().Equals("service"))
                        {
                            oFactura = new FacturiBorderou();
                            oFactura.numeFurnizor = oReader.GetString(0);
                            oFactura.codFurnizor = oReader.GetString(1);
                            oFactura.adresaFurnizor = UtilsAddresses.formatAdresa(oReader.GetString(2));
                            oFactura.sosireFurnizor = oReader.GetString(3);
                            oFactura.plecareFurnizor = oReader.GetString(4);
                            oFactura.codAdresaFurnizor = oReader.GetString(11);
                            oFactura.dataStartCursa = startCursa;

                            oFactura.numeClient = oReader.GetString(5);
                            oFactura.codClient = oReader.GetString(6);
                            oFactura.adresaClient = oReader.GetString(7);
                            oFactura.sosireClient = oReader.GetString(8);
                            oFactura.plecareClient = oReader.GetString(9);
                            oFactura.codAdresaClient = oReader.GetString(10);
                            oFactura.pozitie = "-1";
                            oFactura.nrFactura = "-1";
                            listaFacturi.Add(oFactura);

                        }



                    }

                }


                if (listaFacturi.Count > 1 &&  (listaFacturi[listaFacturi.Count-1].codClient.Length == 4 || listaFacturi[listaFacturi.Count-1].codFurnizor.Length == 4))
                {
                    listaFacturi.RemoveAt(listaFacturi.Count-1);
                }

                oReader.Close();
                oReader.Dispose();

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                serializedResult = serializer.Serialize(listaFacturi);


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




    }
}