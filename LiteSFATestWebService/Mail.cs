using System;
using System.Data;
using System.Data.OracleClient;
using System.Net.Mail;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

namespace LiteSFATestWebService
{
    public class Mail
    {




        public void sendComandaMail(string nrComanda, Sms.TipUser[] useri, string filiala)
        {

            List<String> listMails = Utils.getMail(useri, filiala);

            if (listMails.Count > 0)
            {
                string htmlString = getComandaHTML(nrComanda);

                foreach (string mail in listMails)
                {
                    sendMailAsHTML(htmlString, mail);
                }
            }



        }


        private void sendMailAsHTML(string htmlString, string mailAddress)
        {

            try {
                AlternateView htmlView = AlternateView.CreateAlternateViewFromString(htmlString, null, "text/html");

                MailMessage message = new MailMessage();
                message.From = new MailAddress("comenzi.tableta@arabesque.ro");
                message.To.Add(new MailAddress("florin.brasoveanu@arabesque.ro"));

                message.Subject = "Comanda cu articole din magazin" + " , " + mailAddress;

                message.AlternateViews.Add(htmlView);
                SmtpClient client = new SmtpClient("mail.arabesque.ro");
                client.Send(message);
            }
            catch(Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
        }


        private string getComandaHTML(string nrComanda)
        {

            OracleConnection connection = new OracleConnection();
            OracleCommand cmd = new OracleCommand();
            OracleDataReader oReader = null;

            string htmlString = "";

            try
            {
                using (StreamReader reader = File.OpenText(System.Web.HttpContext.Current.Server.MapPath("~/TemplateOferta.html")))
                {
                    string tableBody = "";



                    //antet oferta
                    string connectionString = DatabaseConnections.ConnectToTestEnvironment();
                    connection.ConnectionString = connectionString;
                    connection.Open();
                    cmd = connection.CreateCommand();
                    cmd.CommandText = " select b.nume, c.nume, a.pers_contact,a.telefon, b.nrtel, a.ul, a.nume_client, a.nrcmdsap, to_char(to_date(a.datac,'yyyymmdd')), a.obstra, " + 
                                      " a.valoare, a.mt from sapprd.zcomhead_tableta a, agenti b, clienti c where " +
                                      " id =:idCmd and b.cod = a.cod_agent and c.cod = a.cod_client ";


                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idCmd", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nrComanda;
                    oReader = cmd.ExecuteReader();

                    string numeClient = "", persContact = "", telContact = "", numeAgent = "", telAgent = "", unitLog = "";
                    string nrcmdsap = "", dataComanda = "", observatii = "", valComanda = "", tipTransport = "";
                    if (oReader.HasRows)
                    {
                        oReader.Read();
                        numeAgent = oReader.GetString(0);
                        numeClient = oReader.GetString(6).Trim() != "" ? oReader.GetString(6) : oReader.GetString(1);
                        persContact = oReader.GetString(2);
                        telContact = oReader.GetString(3);
                        telAgent = oReader.GetString(4);
                        unitLog = oReader.GetString(5);
                        nrcmdsap = oReader.GetString(7);
                        dataComanda = oReader.GetString(8);
                        observatii = oReader.GetString(9);
                        valComanda = oReader.GetDouble(10).ToString("N", new CultureInfo("en-US"));
                        tipTransport = Utils.getDescTipTransport(oReader.GetString(11));
                    }


                    //date filiala
                    string adresaUnitLog = "", telUnitLog = "", faxUnitLog = "", bancaUnitLog = "", contUnitLog = "";
                    string[] tokAdrUnitLog = { "123" ,"323","333","4565","666"};
                    adresaUnitLog = tokAdrUnitLog[0];
                    telUnitLog = tokAdrUnitLog[1];
                    faxUnitLog = tokAdrUnitLog[2];
                    bancaUnitLog = tokAdrUnitLog[3];
                    contUnitLog = tokAdrUnitLog[4];



                    //articole oferta
                    cmd.CommandText = " select decode(length(a.cod),18,substr(a.cod,-8),a.cod), b.nume, a.cant_umb,  a.um from sapprd.zcomdet_tableta a, articole b where " +
                                      " a.id =:idCmd and a.cod = b.cod ";

                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Clear();
                    cmd.Parameters.Add(":idCmd", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                    cmd.Parameters[0].Value = nrComanda;
                    oReader = cmd.ExecuteReader();

                    tableBody = "<tbody> ";

                    int nrArt = 1;
                    string oLinie = "", altColor = "";


                    if (oReader.HasRows)
                    {
                        while (oReader.Read())
                        {
                            altColor = "class='alt'";
                            if ((nrArt + 1) % 2 == 0)
                                altColor = "class='norm'";


                            oLinie = "<tr " + altColor + "><td align='center'>" + nrArt.ToString() + ". </td><td align='left'>" + oReader.GetString(1) + " (" + oReader.GetString(0) + ")" +
                                     "</td><td align='center'>" + oReader.GetDecimal(2) + " </td><td align='center'>" + oReader.GetString(3) + "</td> " +
                                     "<tr>";

                            tableBody += oLinie;

                            nrArt++;
                        }
                    }

                  

                    tableBody += "</tbody>";

                    oReader.Close();
                    oReader.Dispose();

                    htmlString = "<html><body><style type='text/css'>" +
                    ".datagrid table    {       border-collapse: collapse;       text-align: left; width: 100%;    } " +
                    ".datagrid    {     font: normal 12px/150% Courier New, Courier, monospace; " +
                    " background: #fff;overflow: hidden;border: 1px solid #006699;-webkit-border-radius: 3px;" +
                    "-moz-border-radius: 3px; border-radius: 3px; } " +
                    ".datagrid table td, .datagrid table th { padding: 2px 2px; } " +
                    ".datagrid table thead th { background-color:#006699;color:#EECFA1; font-size: 14px;border-left: 1px solid #0070A8;}" +
                    ".datagrid table thead th:first-child{border: none;}" +
                    ".datagrid table tbody td {color:#008B45;border-left: 1px solid #E1EEF4;font-size: 13px;font-weight: normal;}" +
                    ".datagrid table tbody .alt td {background: #E1EEF4;color: #008B45;font-size: 15px;}" +
                    ".datagrid table tbody .norm td {color: #008B45;font-size: 15px;}" +
                    ".datagrid table tbody td:first-child{border-left: none;}" +
                    ".datagrid table tbody tr:last-child td{border-bottom: none;}" +
                    ".datagrid table tfoot td { background-color:#006699;color:#EECFA1; font-weight:bold; font-size: 14px;border-left: 1px solid #0070A8;padding: 2px;} " +
                    ".customText1{font-family: Verdana;font-weight: normal;font-size: 11px;color: #68838B;}" +
                    ".customText2{font-family: Candara;font-weight: normal;font-size: 14px;color: #4F94CD;}" +
                    ".customText3{font-family: Serif;font-weight: bold;font-size: 32px;color: #A66D33;}" +
                    ".customText4{font-family: 'Bookman Old Style', serif;font-weight: normal;font-size: 13px;color: #A66D33;}" +
                    ".customText5{font-family: 'Bookman Old Style', serif;font-weight: bold;font-size: 13px;color: #473C8B;}" +
                    ".customText6{font-family: Verdana;font-weight: normal;font-size: 13px;color: #008B45;} </style>" +
                    " <table align='center' width='90%'>" +
                    "<tr><td>" +
                    "</td></tr>" +
                    "<tr><td><table class='customText2'>" +
                    " </table>" +
                    " </td> " +
                    " <td align = 'left' valign='top'><table class='customText' ><tr><td align='left' class='customText3' colspan='2'>" +
                    "Comanda vanzare</td></tr><tr><td class='customText4'></td><td class='customText4'><br></td></tr> " + 
                    " <tr><td class='customText4'>Data</td><td class='customText4'>" + dataComanda + "</td></tr> " +
                    " <tr><td class='customText4'>Valoare</td><td class='customText4'>" + valComanda + "</td></tr> " +
                    " <tr><td class='customText4'>Transport</td><td class='customText4'>" + tipTransport + "</td></tr> " +
                    " </tr>" +
                    "  </table></td>" +
                    "<td valign='top'><table class='customText2'  align='right'><tr><td>Client</td><td>" + numeClient + "</td>" +
                    "</tr><tr><td>Pers. de contact</td><td>" + persContact + "</td></tr><tr><td>Telefon</td><td>" + telContact + "</td></tr>" +
                    " </table></td></tr><tr><td colspan='3'><br><div class='datagrid'>" +
                    "<table ><thead><tr><th width='3%' align='center'>Nr.<br>crt</th><th width='30%' align='center'>Denumirea produselor sau a serviciilor</th>" +
                    "<th width='5%' align='center'>Cant.</th><th width='10%' align='center'>Um</th>" +
                    "" +
                    "</tr></thead>" +
                    tableBody +
                    "</table></div></td></tr><tr><td colspan='3'><table><tr><td class='customText5'><br>Reprezentant vanzari:" +
                    "</td></tr><tr><td class='customText6'>" + numeAgent + ", tel: " + telAgent + "</td></tr></table>" +
                    "</td></tr>" +
                    "<tr><td colspan='3'><table ><tr><td class='customText5'><br>Observatii:</td>" +
                    "</tr><tr><td class='customText6'>" +
                    observatii +
                    "<br></td></tr></table></td></tr></table>";


                }

            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }


            return htmlString;


        }







    }
}