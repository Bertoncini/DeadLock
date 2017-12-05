namespace DeadLockConsole
{
    using System;
    using System.Collections.Generic;

    class Program
    {
        static void Main(string[] args)
        {
            var lista = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25 };

            Console.WriteLine("Rodando Inserts SEM parallel");
            using (var contexto = new Contexto())
            {
                contexto.TransactionScope(lista);
                contexto.TransactionDapper(lista);
                contexto.InsertProcedure(lista);
            }
            Console.WriteLine("Pressione qualquer botão para continuar");
            Console.ReadKey();

            Console.WriteLine("Rodando Inserts COM parallel");
            using (var contexto = new Contexto())
            {
                contexto.TransactionScopeParallel(lista);
                contexto.TransactionDapperParallel(lista);
                contexto.InsertProcedureParallel(lista);
            }

            Console.WriteLine("Finalizado");

            Console.ReadKey();
        }

    }
}
