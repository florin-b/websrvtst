using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OracleClient;
using System.Globalization;
using System.Linq;
using System.Web;

namespace LiteSFATestWebService
{
    public class OperatiiPreturiBG
    {
        public static double getCmp(OracleConnection conn, string tipAfis, string filiala, string articol, string filialaAgent)
        {
            double cmp = 0;

            if (tipAfis == "3" || tipAfis == "1")
                cmp = calculeazaCmp(conn, filiala, articol, filialaAgent);
            else
                cmp = -1;

            return cmp;

        }


        private static double calculeazaCmp(OracleConnection conn, string filiala, string codArticol, string filialaAgent)
        {
            OracleCommand cmd = null;
            OracleDataReader oReader = null;
            double valoareCmp = 0;

            if (codArticol.Length == 8)
                codArticol = "0000000000" + codArticol;

            string filialaCmp = filialaAgent;



            try
            {
                cmd = conn.CreateCommand();

                cmd.CommandText = " select nvl(to_char(decode(y.lbkum,0,y.verpr,y.salk3/y.lbkum),'99999.9999'),'0') from sapdev.mbew y where " +
                                  " y.mandt='900' and y.matnr=:codArticol  and y.bwkey =:filiala ";


                cmd.Parameters.Clear();
                cmd.Parameters.Add(":codArticol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = formatFullCodArticol(codArticol);

                cmd.Parameters.Add(":filiala", OracleType.VarChar, 12).Direction = ParameterDirection.Input;
                cmd.Parameters[1].Value = filialaCmp;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {

                    oReader.Read();
                    valoareCmp = Double.Parse(oReader.GetString(0).Trim(), CultureInfo.InvariantCulture);

                    double procRedCmp = getProcReducereCmp(conn, codArticol);

                    valoareCmp = valoareCmp * (100 - procRedCmp) / 100;




                }
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }


            return valoareCmp;
        }

        private static string formatFullCodArticol(string codArticol)
        {
            string codArt = codArticol;

            if (codArt.Length == 8)
                codArt = "0000000000" + codArt;

            return codArt;
        }


        public static double getProcReducereCmp(OracleConnection conn, string codArt)
        {

            OracleCommand cmd = null;
            OracleDataReader oReader = null;

            double procReducere = 0;

            try
            {

                cmd = conn.CreateCommand();

                string sqlString = " select nvl(sum(decode(art,'articol',procent)),0) proc_articol, nvl(sum(decode(art,'sintetic',procent)),0) proc_sintetic from ( " +
                                   " select sum(procent)procent, 'articol' art from sapdev.ZSUBCMP p where p.mandt = '900'  and p.spart = (select spart from articole where cod =:articol) " +
                                   " and p.datab <= to_char(sysdate, 'yyyymmdd') and p.datbi >= to_char(sysdate, 'yyyymmdd') " +
                                   " and matnr =:articol group by 'articol' " +
                                   " union " +
                                   " select sum(procent) procent, 'sintetic' art from sapdev.ZSUBCMP p " +
                                   " where p.mandt = '900' and p.spart =(select sintetic from articole where cod =:articol)  and p.datab <= to_char(sysdate, 'yyyymmdd') and p.datbi >= to_char(sysdate, 'yyyymmdd') " +
                                   " and matkl = (select sintetic from articole where cod =:articol) group by 'sintetic') x group by 'articol'";

                cmd.CommandText = sqlString;

                cmd.Parameters.Clear();

                cmd.Parameters.Add(":articol", OracleType.VarChar, 54).Direction = ParameterDirection.Input;
                cmd.Parameters[0].Value = codArt;

                oReader = cmd.ExecuteReader();

                if (oReader.HasRows)
                {
                    oReader.Read();
                    procReducere = oReader.GetDouble(0) != 0 ? oReader.GetDouble(0) : oReader.GetDouble(1);
                }


            }
            catch (Exception ex)
            {
                ErrorHandling.sendErrorToMail(ex.ToString());
            }
            finally
            {
                DatabaseConnections.CloseConnections(oReader, cmd);
            }

            return procReducere;

        }
    }
}