using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.OracleClient;

namespace LiteSFATestWebService
{
    public class OperatiiCLP
    {
        public string saveNewClp(string comanda, string codAgent, string filiala, string depart, bool alertSD)
        {
            string retVal = "-1";


            ErrorHandling.sendErrorToMail(comanda + " , " + codAgent + " , " + filiala + " , " + depart + " , " + alertSD);

            OracleConnection connection = new OracleConnection();
            OracleTransaction transaction = null;

            try
            {

                string[] mainTokenClp = comanda.Split('@');
                string[] antetClpToken = mainTokenClp[0].Split('#');

                string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                connection.ConnectionString = connectionString;
                connection.Open();

                OracleCommand cmd = connection.CreateCommand();

                string query = " insert into sapprd.zclphead(mandt, id, cod_client, cod_agent, ul, ul_dest, depart, status, nrcmdsap, datac, accept1, status_aprov, " +
                               " pers_contact, telefon, adr_livrare, city, region, ketdat, dl, tip_plata, mt, depoz_dest, val_comanda, obs, furn_prod, cod_agent2, " +
                               " fasonate, name1, felmarfa, masa, tipcamion, tipinc) values ('900', pk_clp.nextval, :codCl, :codAg, :ul, :ulDest, :depart, :status, :nrcmdsap , " +
                               " :datac, :accept1, :status_aprov, :perscont, :tel, :adr, :city, :region, :ketdat, ' ', :tipPlata, :tipTransport, :depozDest, 0, " +
                               ":obs ,' ', :codAgent2,:fasonate, :nume, :felmarfa, :masa,:tipcamion,:tipinc) " +
                               " returning id into :id ";

                transaction = connection.BeginTransaction();
                cmd.Transaction = transaction;

                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":codCl", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = antetClpToken[0];

                cmd.Parameters.Add(":codAg", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = codAgent;

                cmd.Parameters.Add(":ul", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[2].Value = filiala;

                cmd.Parameters.Add(":ulDest", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[3].Value = antetClpToken[6];

                cmd.Parameters.Add(":depart", OracleType.VarChar, 6).Direction = ParameterDirection.Input;
                cmd.Parameters[4].Value = depart;

                cmd.Parameters.Add(":status", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[5].Value = "0";

                cmd.Parameters.Add(":nrcmdsap", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                cmd.Parameters[6].Value = " ";

                cmd.Parameters.Add(":datac", OracleType.VarChar, 18).Direction = ParameterDirection.Input;
                cmd.Parameters[7].Value = getCurrentDate();

                cmd.Parameters.Add(":accept1", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                string valSD = "1";
                if (alertSD)
                    valSD = "X";
                cmd.Parameters[8].Value = valSD;

                string status_aprov = "0";
                if (alertSD || antetClpToken[9] == "TERT" || antetClpToken[12] == "false")
                    status_aprov = "1";

                cmd.Parameters.Add(":status_aprov", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[9].Value = status_aprov;

                cmd.Parameters.Add(":perscont", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[10].Value = antetClpToken[4];

                cmd.Parameters.Add(":tel", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[11].Value = antetClpToken[5];

                cmd.Parameters.Add(":adr", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[12].Value = antetClpToken[3];

                cmd.Parameters.Add(":city", OracleType.VarChar, 75).Direction = ParameterDirection.Input;
                cmd.Parameters[13].Value = antetClpToken[2];

                string varRegion = antetClpToken[1];
                if (varRegion.Trim().Equals(""))
                    varRegion = " ";
                cmd.Parameters.Add(":region", OracleType.VarChar, 9).Direction = ParameterDirection.Input;
                cmd.Parameters[14].Value = varRegion;

                cmd.Parameters.Add(":ketdat", OracleType.VarChar, 30).Direction = ParameterDirection.Input;
                string[] dataLivareClp = antetClpToken[7].Split('.');

                cmd.Parameters[15].Value = dataLivareClp[2] + dataLivareClp[1] + dataLivareClp[0];

                cmd.Parameters.Add(":tipPlata", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[16].Value = antetClpToken[8];

                cmd.Parameters.Add(":tipTransport", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[17].Value = antetClpToken[9];

                cmd.Parameters.Add(":depozDest", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[18].Value = antetClpToken[10];

                cmd.Parameters.Add(":codAgent2", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[19].Value = antetClpToken[11];

                cmd.Parameters.Add(":fasonate", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                string paramFasonate = " ";
                if (antetClpToken[12] == "true")
                {
                    paramFasonate = "X";
                }
                cmd.Parameters[20].Value = paramFasonate;

                cmd.Parameters.Add(":nume", OracleType.VarChar, 90).Direction = ParameterDirection.Input;
                cmd.Parameters[21].Value = antetClpToken[13];

                cmd.Parameters.Add(":obs", OracleType.VarChar, 150).Direction = ParameterDirection.Input;
                cmd.Parameters[22].Value = antetClpToken[14].Length > 0 ? antetClpToken[14] : " ";

                cmd.Parameters.Add(":felmarfa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[23].Value = antetClpToken[15];

                cmd.Parameters.Add(":masa", OracleType.VarChar, 180).Direction = ParameterDirection.Input;
                cmd.Parameters[24].Value = antetClpToken[16];

                cmd.Parameters.Add(":tipcamion", OracleType.VarChar, 45).Direction = ParameterDirection.Input;
                cmd.Parameters[25].Value = antetClpToken[17];

                cmd.Parameters.Add(":tipinc", OracleType.VarChar, 24).Direction = ParameterDirection.Input;
                cmd.Parameters[26].Value = antetClpToken[18];


                OracleParameter idCmd = new OracleParameter("id", OracleType.Number);
                idCmd.Direction = ParameterDirection.Output;
                cmd.Parameters.Add(idCmd);

                cmd.ExecuteNonQuery();




                //inserare articole comanda
                string[] articoleCmdTokenClp;



                int artLen = mainTokenClp.Length - 2;
                int pozArt = 0;
                string tokenArtClp = "";
                for (int i = 0; i < artLen; i++)
                {

                    pozArt = (i + 1) * 10;


                    tokenArtClp = mainTokenClp[i + 1];
                    articoleCmdTokenClp = tokenArtClp.Split('#');
                    string codArtClp = articoleCmdTokenClp[0];
                    if (codArtClp.Length == 8)
                        codArtClp = "0000000000" + codArtClp;


                    query = " insert into sapprd.zclpdet(mandt,id,poz,status,cod,cantitate,umb,depoz) " +
                            " values ('900'," + idCmd.Value + ",'" + pozArt + "','0','" + codArtClp + "',:cantArt, " +
                            "'" + articoleCmdTokenClp[2] + "','" + articoleCmdTokenClp[3] + "' ) ";


                    cmd.CommandText = query;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();

                    cmd.Parameters.Add(":cantArt", OracleType.Number, 13).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = Double.Parse(articoleCmdTokenClp[1]);

                    cmd.ExecuteNonQuery();


                }

                transaction.Commit();

               

                SapWsClp.ZCLP_WEBSERVICE webServiceClp = null;

                webServiceClp = new SapWsClp.ZCLP_WEBSERVICE();

                System.Net.NetworkCredential nc = new System.Net.NetworkCredential(Service1.getUser(), Service1.getPass());
                webServiceClp.Credentials = nc;
                webServiceClp.Timeout = 300000;

                SapWsClp.ZcreazaSto inParam = new SapWsClp.ZcreazaSto();
                inParam.VId = Convert.ToDecimal(idCmd.Value);

                SapWsClp.ZcreazaStoResponse outParam = webServiceClp.ZcreazaSto(inParam);

                retVal = outParam.VOk.ToString() + " , " + outParam.VMess.ToString();

                webServiceClp.Dispose();
               

                //nu este nevoie de aprobare, se trimite mail de instiintare
                if (status_aprov.Equals("0"))
                {
                    if (!Service1.isClpTransferIntreFiliale(idCmd.Value.ToString()))
                        Service1.sendAlertMailCreareClp(idCmd.Value.ToString());
                }
                //sf. alert


            }
            catch (Exception ex)
            {
                retVal = "-1";
                ErrorHandling.sendErrorToMail(ex.ToString() + " " + comanda);
            }
            finally
            {
                connection.Close();
                connection.Dispose();
            }




            return retVal;
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


    }




}