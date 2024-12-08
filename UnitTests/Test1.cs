
using DiscordBot;
using System.Diagnostics.Contracts;

namespace UnitTests;

[TestClass]
public sealed class Test1 {
    [TestMethod]
    [Timeout(1000)]
    public void TestMethod1() {
        Console.WriteLine("new cs");
        var cs = new ConstraintSolver(Util.basicFractalSquad);


        Console.WriteLine("add players");
        cs.AddPlayerRole("healPlayer", Util.healAlac);
        cs.AddPlayerRole("healPlayer", Util.healQuick);

        cs.AddPlayerRole("boonPlayer", Util.alacDPS);
        cs.AddPlayerRole("boonPlayer", Util.quickDPS);

        cs.AddPlayerRole("DPSPlayer1", Util.DPS);
        cs.AddPlayerRole("DPSPlayer2", Util.DPS);
        cs.AddPlayerRole("DPSPlayer3", Util.DPS);

        Console.WriteLine("get player roles)");
        var roles = cs.GetPlayerRoles();

        Assert.AreEqual(5, roles.Count);

        foreach (var (player, role) in roles) {
            Console.WriteLine("{0} is {1}", player, role.name);

            if (player == "healPlayer") {
                Assert.IsTrue(role.name == Util.healAlac.name || role.name == Util.healQuick.name);
            }
            if (player == "boonPlayer") {
                Assert.IsTrue(role.name == Util.alacDPS.name || role.name == Util.quickDPS.name);
            }
            if (player == "DPSPlayer1" ||
                player == "DPSPlayer2" ||
                player == "DPSPlayer3") {
                Assert.IsTrue(role.name == Util.DPS.name);
            }
        }

    }
}
