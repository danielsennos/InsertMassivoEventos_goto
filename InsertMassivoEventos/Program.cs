using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InsertMassivoEventos
{
    class Program
    {
        static void Main(string[] args)
        {
            Repository repo = new Repository();

            Console.WriteLine("Id (inclusivo) da primeira cerca a ser vinculada aos eventos");
            long idCercaMin = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Id (inclusivo) da última cerca a ser vinculada aos eventos");
            long idCercaMax = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Id do tipo do evento a ser cadastrado:");
            long idTipoEvento= Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Pressione qualquer tecla para proseguir");
            Console.ReadKey();


            Console.WriteLine("Iniciando Inclusão...");


            ArrayList cercas_list = repo.GetCercas(idCercaMin, idCercaMax);
            ArrayList viatura_list = repo.GetViaturas();
            repo.CreateEvents(viatura_list, cercas_list, idTipoEvento);

            Console.WriteLine("Inclusão Finalizada...");
            Console.ReadKey();

        }
    }
}
