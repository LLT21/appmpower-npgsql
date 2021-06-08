using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Npgsql;

namespace appMpower
{
   public static class RawDbNpgsql
   {
      private const int MaxBatch = 500;
      private static Random _random = new Random();
      private static string[] _queriesMultipleRows = new string[MaxBatch + 1];

      public static async Task<World> LoadSingleQueryRow()
      {
         Console.WriteLine("before new connection");
         using var pooledConnection = new NpgsqlConnection(ConnectionStrings.NpgSqlConnection);
         Console.WriteLine("before open connection");
         await pooledConnection.OpenAsync();

         Console.WriteLine("before create command");
         var (pooledCommand, _) = CreateReadCommand(pooledConnection);

         using (pooledCommand)
         {
            Console.WriteLine("before reading single row");
            return await ReadSingleRow(pooledCommand);
         }
      }

      public static async Task<World[]> LoadMultipleQueriesRows(int count)
      {
         var worlds = new World[count];

         using var pooledConnection = new NpgsqlConnection(ConnectionStrings.OdbcConnection);
         await pooledConnection.OpenAsync();

         var (pooledCommand, dbDataParameter) = CreateReadCommand(pooledConnection);

         using (pooledCommand)
         {
            for (int i = 0; i < count; i++)
            {
               worlds[i] = await ReadSingleRow(pooledCommand);
               dbDataParameter.Value = _random.Next(1, 10001);
            }
         }

         return worlds;
      }

      public static async Task<List<Fortune>> LoadFortunesRows()
      {
         var fortunes = new List<Fortune>();

         using var pooledConnection = new NpgsqlConnection(ConnectionStrings.OdbcConnection);
         await pooledConnection.OpenAsync();

         var pooledCommand = new NpgsqlCommand("SELECT id, message FROM fortune", pooledConnection);

         using (pooledCommand)
         {
            using var dataReader = await pooledCommand.ExecuteReaderAsync(CommandBehavior.SingleResult);

            while (await dataReader.ReadAsync())
            {
               fortunes.Add(new Fortune
               (
                   id: dataReader.GetInt32(0),
                   message: dataReader.GetString(1)
               ));
            }
         }

         fortunes.Add(new Fortune(id: 0, message: "Additional fortune added at request time."));
         fortunes.Sort();

         return fortunes;
      }

      public static async Task<World[]> LoadMultipleUpdatesRows(int count)
      {
         var worlds = new World[count];

         using var pooledConnection = new NpgsqlConnection(ConnectionStrings.OdbcConnection);
         await pooledConnection.OpenAsync();

         var (queryCommand, dbDataParameter) = CreateReadCommand(pooledConnection);

         using (queryCommand)
         {
            for (int i = 0; i < count; i++)
            {
               worlds[i] = await ReadSingleRow(queryCommand);
               dbDataParameter.Value = _random.Next(1, 10001);
            }
         }

         var updateCommand = new NpgsqlCommand(PlatformBenchmarks.BatchUpdateString.Query(count), pooledConnection);

         using (updateCommand)
         {
            var ids = PlatformBenchmarks.BatchUpdateString.Ids;
            var randoms = PlatformBenchmarks.BatchUpdateString.Randoms;
            var jds = PlatformBenchmarks.BatchUpdateString.Jds;

            for (int i = 0; i < count; i++)
            {
               var randomNumber = _random.Next(1, 10001);

               updateCommand.Parameters.Add(new NpgsqlParameter<int>(parameterName: ids[i], value: worlds[i].Id));
               updateCommand.Parameters.Add(new NpgsqlParameter<int>(parameterName: randoms[i], value: randomNumber));

               worlds[i].RandomNumber = randomNumber;
            }

            for (int i = 0; i < count; i++)
            {
               updateCommand.Parameters.Add(new NpgsqlParameter<int>(parameterName: jds[i], value: worlds[i].Id));
            }

            await updateCommand.ExecuteNonQueryAsync();
         }

         return worlds;
      }

      private static (NpgsqlCommand pooledCommand, IDbDataParameter dbDataParameter) CreateReadCommand(NpgsqlConnection pooledConnection)
      {
         var pooledCommand = new NpgsqlCommand("SELECT id, randomnumber FROM world WHERE id = @Id", pooledConnection);
         var dbDataParameter = new NpgsqlParameter<int>(parameterName: "@Id", value: _random.Next(1, 10001));

         pooledCommand.Parameters.Add(dbDataParameter);

         return (pooledCommand, dbDataParameter);
      }

      private static async Task<World> ReadSingleRow(NpgsqlCommand pooledCommand)
      {
         using var dataReader = await pooledCommand.ExecuteReaderAsync(CommandBehavior.SingleRow);
         await dataReader.ReadAsync();

         return new World
         {
            Id = dataReader.GetInt32(0),
            RandomNumber = dataReader.GetInt32(1)
         };
      }

      public static async Task<World[]> ReadMultipleRows(int count)
      {
         int j = 0;
         var ids = PlatformBenchmarks.BatchUpdateString.Ids;
         var worlds = new World[count];
         string queryString;

         if (_queriesMultipleRows[count] != null)
         {
            queryString = _queriesMultipleRows[count];
         }
         else
         {
            var stringBuilder = PlatformBenchmarks.StringBuilderCache.Acquire();

            for (int i = 0; i < count; i++)
            {
               stringBuilder.Append("SELECT id, randomnumber FROM world WHERE id =?;");
            }

            queryString = _queriesMultipleRows[count] = PlatformBenchmarks.StringBuilderCache.GetStringAndRelease(stringBuilder);
         }

         using var pooledConnection = new NpgsqlConnection(ConnectionStrings.OdbcConnection);
         await pooledConnection.OpenAsync();

         using var pooledCommand = new NpgsqlCommand(queryString, pooledConnection);

         for (int i = 0; i < count; i++)
         {
            pooledCommand.Parameters.Add(new NpgsqlParameter<int>(parameterName: ids[i], value: _random.Next(1, 10001)));
         }

         using var dataReader = await pooledCommand.ExecuteReaderAsync(CommandBehavior.Default);

         do
         {
            await dataReader.ReadAsync();

            worlds[j] = new World
            {
               Id = dataReader.GetInt32(0),
               RandomNumber = dataReader.GetInt32(1)
            };

            j++;
         } while (await dataReader.NextResultAsync());

         return worlds;
      }
   }
}