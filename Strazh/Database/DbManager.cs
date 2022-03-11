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
                                          "ON CREATE SET a.pk = $nodeAPk, a.fullName = $nodeAFullName, a.name = $nodeAName " +
                                          "ON MATCH SET a.pk = $nodeAPk, a.fullName = $nodeAFullName, a.name = $nodeAName " +
                                          "MERGE (b: {1} {{ pk: $nodeBPk }}) " +
                                          "ON CREATE SET b.pk = $nodeBPk, b.fullName = $nodeBFullName, b.name = $nodeBName " +
                                          "ON MATCH SET b.pk = $nodeBPk, b.fullName = $nodeBFullName, b.name = $nodeBName MERGE (a)-[:{2}]->(b);";
        
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
                    Dictionary<string, object> parameters = new()
                    {
                        ["nodeAPk"] = triple.NodeA.Pk,
                        ["nodeBPk"] = triple.NodeB.Pk,
                        ["nodeAFullName"] = triple.NodeA.FullName,
                        ["nodeBFullName"] = triple.NodeB.FullName,
                        ["nodeAName"] = triple.NodeA.Name,
                        ["nodeBName"] = triple.NodeB.Name,
                    };
                    var query = string.Format(MergeQuery, triple.NodeA.Label, triple.NodeB.Label,
                        triple.Relationship.Type);
                    await session.RunAsync(query, parameters);
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