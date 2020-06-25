using Oracle.DataAccess.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsertMassivoEventos
{
    class Repository
    {
        //private readonly string _connectionString = "Data Source=SYNDES;User ID=avl2;password=avldesenv";

        private readonly string _connectionString = "Data Source=AVL2PROD;User ID=avl2;password=avlsynapsis";

       
        public ArrayList GetViaturas()
        {

            string query = $@"SELECT ID FROM GOTO_ENGEMIX.AVL_VIATURA WHERE ID_CLIENTE = 134 AND STATUS = 'A'";

            ArrayList viaturas_list = new ArrayList();

            using (var connection = new OracleConnection(_connectionString))
            {
                OracleDataAdapter adapter = new OracleDataAdapter();
                adapter.SelectCommand = new OracleCommand(query, connection);

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                foreach(DataRow row in dt.Rows)
                {
                    viaturas_list.Add(Convert.ToInt32(row["id"]));
                }
            }
            return viaturas_list;
        }

        public ArrayList GetCercas(long idCercaMin, long idCercaMax)
        {

            string query = $@"SELECT ID FROM GOTO_ENGEMIX.AVL_CERCA_ELETRONICA WHERE ID_CLIENTE = 134 AND ID BETWEEN {idCercaMin} AND {idCercaMax}";

            ArrayList cercas_list = new ArrayList();

            using (var connection = new OracleConnection(_connectionString))
            {
                OracleDataAdapter adapter = new OracleDataAdapter();
                adapter.SelectCommand = new OracleCommand(query, connection);

                DataTable dt = new DataTable();
                adapter.Fill(dt);

                foreach (DataRow row in dt.Rows)
                {
                    cercas_list.Add(Convert.ToInt32(row["id"]));
                }
            }
            return cercas_list;
        }

        public void CreateEvents(ArrayList viatura_list, ArrayList cercas_list, long idTipoEvento)
        {
            try {
                using (var connection = new OracleConnection(_connectionString))
                {
                    connection.Open();
                    var transaction = connection.BeginTransaction();
                    try
                    {                        
                        foreach (var cerca_id in cercas_list)
                        {
                            Console.WriteLine($"Cerca id: {cerca_id}");

                            long Goto_Events_Id = 0;
                            long Goto_Events_Filter_Id = 0;

                            using (var cmdNotiScope = connection.CreateCommand())
                            {
                                CultureInfo cult = new CultureInfo("pt-BR");

                                cmdNotiScope.CommandText = String.Format($@"INSERT INTO GOTO_ENGEMIX.goto_events (user_id, name, created, tipo_evento) VALUES (:user_id, (select nome from GOTO_ENGEMIX.avl_cerca_eletronica where id = {cerca_id}), SYSDATE, :tipo_evento) RETURNING id INTO :id");
                                cmdNotiScope.Parameters.Add("user_id", 5057);
                                cmdNotiScope.Parameters.Add("tipo_evento", idTipoEvento);

                                cmdNotiScope.Parameters.Add("id", OracleDbType.Decimal, ParameterDirection.Output);
                                cmdNotiScope.ExecuteNonQuery();
                                Goto_Events_Id = long.Parse(cmdNotiScope.Parameters["id"].Value.ToString());

                            }
                            using (var cmdNotiScope = connection.CreateCommand())
                            {
                                CultureInfo cult = new CultureInfo("pt-BR");
                                cmdNotiScope.CommandText = String.Format($@"INSERT INTO GOTO_ENGEMIX.goto_event_filters (event_id) VALUES (:event_id) RETURNING id INTO :id");
                                cmdNotiScope.Parameters.Add("event_id", Goto_Events_Id);

                                cmdNotiScope.Parameters.Add("id", OracleDbType.Decimal, ParameterDirection.Output);
                                cmdNotiScope.ExecuteNonQuery();
                                Goto_Events_Filter_Id = long.Parse(cmdNotiScope.Parameters["id"].Value.ToString());

                            }
                            using (var cmdNotiScope = connection.CreateCommand())
                            {
                                CultureInfo cult = new CultureInfo("pt-BR");
                                cmdNotiScope.CommandText = String.Format($@"INSERT INTO GOTO_ENGEMIX.goto_monitored_point_filters (id) VALUES (:id)");
                                cmdNotiScope.Parameters.Add("id", Goto_Events_Filter_Id);

                                cmdNotiScope.ExecuteNonQuery();

                            }
                            foreach (var id_viatura in viatura_list)
                            {
                                Console.WriteLine($"Cerca id: {cerca_id} , viatura id: {id_viatura}");
                                using (var cmdNotiScope = connection.CreateCommand())
                                {
                                    CultureInfo cult = new CultureInfo("pt-BR");
                                    cmdNotiScope.CommandText = String.Format($@"INSERT INTO GOTO_ENGEMIX.goto_mopf_scopes (monitored_point_filter_id, monitored_point_id) 
                                                                        VALUES (:goto_event_filter_id, :monitored_point_id)");
                                    cmdNotiScope.Parameters.Add("goto_event_filter_id", Goto_Events_Filter_Id);
                                    cmdNotiScope.Parameters.Add("monitored_point_id", id_viatura);
                                    cmdNotiScope.ExecuteNonQuery();
                                }
                            }
                            using (var cmdNotiScope = connection.CreateCommand())
                            {
                                CultureInfo cult = new CultureInfo("pt-BR");
                                cmdNotiScope.CommandText = String.Format($@"INSERT INTO GOTO_ENGEMIX.goto_perimeter_filters (id, direction) 
                                                                                VALUES (:goto_event_filter_id, :direction)");
                                cmdNotiScope.Parameters.Add("goto_event_filter_id", Goto_Events_Filter_Id);
                                cmdNotiScope.Parameters.Add("direction", "1"); //0 - ambos 1- dentro 2-fora
                                cmdNotiScope.ExecuteNonQuery();

                            }
                            using (var cmdNotiScope = connection.CreateCommand())
                            {
                                CultureInfo cult = new CultureInfo("pt-BR");
                                cmdNotiScope.CommandText = String.Format($@"INSERT INTO GOTO_ENGEMIX.goto_pefi_peri (perimeter_filter_id, perimeter_id) 
                                                                            VALUES (:perimeter_filter_id, :perimeter_id)");
                                cmdNotiScope.Parameters.Add("perimeter_filter_id", Goto_Events_Filter_Id);
                                cmdNotiScope.Parameters.Add("perimeter_id", cerca_id);
                                cmdNotiScope.ExecuteNonQuery();

                            }
                            GC.Collect();
                        }
                    }
                    catch (Exception ex) { transaction.Rollback(); }
                    finally { transaction.Commit(); }
                

                }


            } catch (Exception ex) { }
            

        }

       

    }
}
