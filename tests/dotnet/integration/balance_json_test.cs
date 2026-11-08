using System.IO;
using System.Text.Json;
using NUnit.Framework;
using Vamp.Core.Config;

namespace Vamp.Tests.Integration
{
    /// <summary>Validates the shipped balance.json (file I/O, so it lives outside the Unity unit tests).</summary>
    public class BalanceJsonTest
    {
        private static string JsonPath =>
            Path.Combine(TestContext.CurrentContext.TestDirectory, "../../../../../src/Brasslight/Assets/StreamingAssets/balance.json");

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { IncludeFields = true };

        [Test]
        public void test_shipped_balance_json_resolves_without_errors()
        {
            Assert.IsTrue(File.Exists(JsonPath), "balance.json missing: " + JsonPath);
            var cfg = JsonSerializer.Deserialize<BalanceConfig>(File.ReadAllText(JsonPath), Options);
            CollectionAssert.IsEmpty(cfg.Resolve());
        }

        /// <summary>Regenerates balance.json from the code defaults when BRASSLIGHT_WRITE_BALANCE=1.</summary>
        [Test]
        public void tool_write_balance_json_from_defaults()
        {
            if (System.Environment.GetEnvironmentVariable("BRASSLIGHT_WRITE_BALANCE") != "1") Assert.Ignore("generator only");
            var node = JsonSerializer.SerializeToNode(BalanceConfig.CreateDefault(), Options);
            StripRuntimeFields(node);
            var json = node.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(JsonPath));
            File.WriteAllText(JsonPath, json);
        }

        /// <summary>Removes [NonSerialized] resolved fields (Kind/Effect) that System.Text.Json does not skip.</summary>
        private static void StripRuntimeFields(System.Text.Json.Nodes.JsonNode node)
        {
            if (node is System.Text.Json.Nodes.JsonObject obj)
            {
                obj.Remove("Kind");
                obj.Remove("Effect");
                foreach (var kv in obj) StripRuntimeFields(kv.Value);
            }
            else if (node is System.Text.Json.Nodes.JsonArray arr)
                foreach (var item in arr) StripRuntimeFields(item);
        }
    }
}
