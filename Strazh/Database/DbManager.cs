using Neo4j.Driver;
using System;
using System.Threading.Tasks;
using Strazh.Domain;
using System.Collections.Generic;
using static Strazh.Analysis.AnalyzerConfig;

namespace Strazh.Database
{
    public static class DbManager
    {
        private const string CONNECTION = "bolt://localhost:7687";

        private const string MergeQuery = "MERGE (a: {0} {{ pk: $nodeAPk }}) " +
                                          "ON CREATE SET {1} " +
                                          "ON MATCH SET {1} " +
                                          "MERGE (b: {2} {{ pk: $nodeBPk }}) " +
                                          "ON CREATE SET {3} " +
                                          "ON MATCH SET {3} MERGE (a)-[:{4}]->(b);";
        
        public static async Task InsertData(IList<Triple> triples, CredentialsConfig credentials, bool isDelete)
        {
            if (credentials == null)
            {
                throw new ArgumentException($"Please, provide credentials.");
            }
            Console.WriteLine($"Code Knowledge Graph use \"{credentials.Database}\" Neo4j database.");
            var driver = GraphDatabase.Driver(CONNECTION, AuthTokens.Basic(credentials.User, credentials.Password));
            var session = driver.AsyncSession(o => o.WithDatabase(credentials.Database));
            try
            {
                if (isDelete)
                {
                    Console.WriteLine($"Deleting graph data of \"{credentials.Database}\" database...");
                    await session.RunAsync("MATCH (n) DETACH DELETE n;");
                    Console.WriteLine($"Deleting graph data of \"{credentials.Database}\" database complete.");
                }
                Console.WriteLine($"Processing {triples.Count} triples...");
                foreach (var triple in triples)
                {
                    var (setA, aParameters) = triple.NodeA.GetSetQueryAndParameters("a");
                    var (setB, bParameters) = triple.NodeB.GetSetQueryAndParameters("b");

                    foreach (var kvp in bParameters)
                    {
                        aParameters.Add(kvp);
                    }
                    
                    var query = $"MERGE (a: {triple.NodeA.Label} {{ pk: $nodeAPk }}) " +
                                $"ON CREATE SET {setA} " +
                                $"ON MATCH SET {setA} " +
                                $"MERGE (b: {triple.NodeB.Label} {{ pk: $nodeBPk }}) " +
                                $"ON CREATE SET {setB} " +
                                $"ON MATCH SET {setB} MERGE (a)-[:{triple.Relationship.Type}]->(b);";
                    
                    await session.RunAsync(query, aParameters);
                }
                Console.WriteLine($"Processing {triples.Count} triples complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                await session.CloseAsync();
                await driver.CloseAsync();
            }
        }
    }
}