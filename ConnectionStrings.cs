namespace appMpower
{
   public static class ConnectionStrings
   {
      public const string NpgSqlConnection = "Server=127.0.0.1;Port=5432;Database=hello_world;User Id=benchmarkdbuser;Password=benchmarkdbpass;Maximum Pool Size=256;NoResetOnClose=true;Enlist=false;Max Auto Prepare=3";
      public const string OdbcConnection = "Driver={PostgreSQL};Server=127.0.0.1;Port=5432;Database=hello_world;Uid=benchmarkdbuser;Pwd=benchmarkdbpass;UseServerSidePrepare=1;Pooling=false";

      //public const string OdbcConnection = "Driver={PostgreSQL};Server=host.docker.internal;Port=5432;Database=hello_world;Uid=benchmarkdbuser;Pwd=benchmarkdbpass;UseServerSidePrepare=1;Pooling=false";
      //public const string OdbcConnection = "Driver={PostgreSQL};Server=tfb-database;Database=hello_world;Uid=benchmarkdbuser;Pwd=benchmarkdbpass;UseServerSidePrepare=1;Pooling=false";
   }
}