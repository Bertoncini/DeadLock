namespace DeadLockConsole
{
    using System;
    using System.Collections.Generic;
    using Dapper;
    using System.Threading.Tasks;
    using System.Data.SqlClient;
    using System.Transactions;
    using System.Threading;

    class Contexto : IDisposable
    {
        const string _stringConection = "Data Source=(local);Initial Catalog=master;Integrated Security=True";
        const string _querySelect = "select Qtd = Count(1), Tipo = Campo2 from dbTesteDeadLock.dbo.testeDeadLock group by campo2";
        const string _insertInventarioItem = "insert into dbTesteDeadLock.dbo.testeDeadLock values (@campo1, @campo2)";

        string CriaBancoDeDados => @"
    if not exists(select * from sys.databases where name = 'dbTesteDeadLock')
    begin
	    Create DataBase dbTesteDeadLock
    end
";

        string CriaTabela => @"
    if not exists(select * from dbTesteDeadLock.sys.tables where name = 'testeDeadLock')
    begin
	    Create Table dbTesteDeadLock.dbo.testeDeadLock 
	    (
	        campo1 bigint,
            campo2 nvarchar(200)
	    )
    end
";

        string DropProcedure => @"
    if Exists(select * from sys.procedures where name ='usp_InsertTesteDeadLock')
    begin
	    Drop Procedure	usp_InsertTesteDeadLock
    end
";

        string CriaProcedureInsert => @"
    Create Procedure usp_InsertTesteDeadLock(@campo1 bigint, @campo2 nvarchar(200))
    as
    Begin
	    Begin tran
		    Insert dbTesteDeadLock.dbo.testeDeadLock values (@campo1, @campo2)
	    commit
    end    
";

        string DropBanco => @"
    if exists(select * from sys.databases where name = 'dbTesteDeadLock')
    begin
	    Drop DataBase dbTesteDeadLock
    end
";

        public Contexto()
        {
            using (var con = new SqlConnection(_stringConection))
            {
                con.Open();
                con.Execute(CriaBancoDeDados);
                con.Execute(CriaTabela);
                con.Execute(DropProcedure);
                con.Execute(CriaProcedureInsert);
            }
        }

        public void Dispose()
        {
            using (var con = new SqlConnection(_stringConection))
            {
                con.Open();
                Console.WriteLine("As mensagem anteriores estão sendo apagadas.");
                Thread.Sleep(new TimeSpan(0, 0, 3));
                Console.Clear();
                var resultado = con.Query<Resultado>(_querySelect);

                foreach (var item in resultado)
                    Console.WriteLine($"Resultado: {item.Tipo} foi inserido {item.Qtd}");

                con.Execute(DropBanco);
                con.Execute(DropProcedure);
            }
        }

        public void InsertProcedureParallel(List<int> lista)
        {
            Parallel.ForEach(lista, x =>
            {
                try
                {
                    using (var con = new SqlConnection(_stringConection))
                    {
                        con.Open();
                        con.Execute(_querySelect);
                        var insertInventarioItem = $"usp_InsertTesteDeadLock {x}, 'insertProcedure'";
                        con.Execute(insertInventarioItem);
                        Console.WriteLine("Insert com Proc Begin tran");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro com Proc Begin tran " + ex.Message);
                }
            });
        }

        public void InsertProcedure(List<int> lista)
        {
            lista.ForEach(x =>
            {
                try
                {
                    using (var con = new SqlConnection(_stringConection))
                    {
                        con.Open();
                        con.Execute(_querySelect);
                        var insertInventarioItem = $"usp_InsertTesteDeadLock {x}, 'insertProcedure'";
                        con.Execute(insertInventarioItem);
                        Console.WriteLine("Insert com Proc Begin tran");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro com Proc Begin tran " + ex.Message);
                }
            });
        }

        public void TransactionDapperParallel(List<int> lista)
        {

            Parallel.ForEach(lista, x =>
            {
                try
                {
                    using (var con = new SqlConnection(_stringConection))
                    {
                        con.Open();
                        using (var tran = con.BeginTransaction())
                        {
                            con.Execute(_querySelect, transaction: tran);
                            con.Execute(_insertInventarioItem, new { campo1 = x, campo2 = "TransactionDapper" }, tran);
                            tran?.Commit();
                            Console.WriteLine("Insert com Dapper Transaction");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro com Dapper Transaction " + ex.Message);
                }
            });
        }

        public void TransactionDapper(List<int> lista)
        {
            lista.ForEach(x =>
            {
                try
                {
                    using (var con = new SqlConnection(_stringConection))
                    {
                        con.Open();
                        using (var tran = con.BeginTransaction())
                        {
                            con.Execute(_querySelect, transaction: tran);
                            con.Execute(_insertInventarioItem, new { campo1 = x, campo2 = "TransactionDapper" }, tran);
                            tran?.Commit();
                            Console.WriteLine("Insert com Dapper Transaction");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro com Dapper Transaction " + ex.Message);
                }
            });
        }

        public void TransactionScopeParallel(List<int> lista)
        {
            Parallel.ForEach(lista, x =>
            {
                try
                {
                    using (var tran = new TransactionScope())
                    {
                        using (var con = new SqlConnection(_stringConection))
                        {
                            con.Open();
                            con.Execute(_querySelect);
                            con.Execute(_insertInventarioItem, new { campo1 = x, campo2 = "TransactionScope" });
                            Console.WriteLine("Insert com TransactionScope");
                            tran.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro com TransactionScope " + ex.Message);
                }
            });
        }

        public void TransactionScope(List<int> lista)
        {
            lista.ForEach(x =>
            {
                try
                {
                    using (var tran = new TransactionScope())
                    {
                        using (var con = new SqlConnection(_stringConection))
                        {
                            con.Open();
                            con.Execute(_querySelect);
                            con.Execute(_insertInventarioItem, new { campo1 = x, campo2 = "TransactionScope" });
                            Console.WriteLine("Insert com TransactionScope");
                            tran.Complete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro com TransactionScope " + ex.Message);
                }
            });
        }

    }
}
