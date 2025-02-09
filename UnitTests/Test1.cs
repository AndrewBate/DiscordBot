﻿
using DiscordBot;
using System.Diagnostics.Contracts;

namespace UnitTests;

[TestClass]
public sealed class Test1 {
    [TestMethod]
    [Timeout(1000)]
    public void CheckEasyFractalTeam() {
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

    [TestMethod]
    [Timeout(1000)]
    public void CheckImpossibleTeam() {
        var cs = new ConstraintSolver(Util.basicFractalSquad);


        Console.WriteLine("add players");
        cs.AddPlayerRole("healPlayer1", Util.healAlac);
        cs.AddPlayerRole("healPlayer1", Util.healQuick);

        cs.AddPlayerRole("healPlayer2", Util.healAlac);
        cs.AddPlayerRole("healPlayer2", Util.healQuick);


        cs.AddPlayerRole("DPSPlayer1", Util.DPS);
        cs.AddPlayerRole("DPSPlayer2", Util.DPS);
        cs.AddPlayerRole("DPSPlayer3", Util.DPS);

        Console.WriteLine("get player roles)");
        var roles = cs.GetPlayerRoles();
        Assert.AreEqual(0, roles.Count);
    }

    [TestMethod]
    [Timeout(1000)]
    public void CheckOversubscibedSquad() {
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
        cs.AddPlayerRole("DPSPlayer4", Util.DPS);

        Console.WriteLine("get player roles)");
        var roles = cs.GetPlayerRoles();

        Assert.AreEqual(0, roles.Count);
    }



    [TestMethod]
    [Timeout(1000)]
    public void CheckMixedSquad() {
        var cs = new ConstraintSolver(Util.basicRaidSquad);

        for (int i = 0; i < 10; i++) {
            var playerName = "player" + i.ToString();

            cs.AddPlayerRole(playerName, Util.alacDPS);
            cs.AddPlayerRole(playerName, Util.quickDPS);
            cs.AddPlayerRole(playerName, Util.healAlac);
            cs.AddPlayerRole(playerName, Util.healQuick);
            cs.AddPlayerRole(playerName, Util.DPS);
        }

        Console.WriteLine("get player roles)");
        var roles = cs.GetPlayerRoles();

        Assert.AreEqual(10, roles.Count);

        int dpsCount = 0;
        int boonDPScount = 0;
        int healCount = 0;
        int alacCount = 0;
        int quickCount = 0;

        foreach (var (player, role) in roles) {
            var rname = role.name;
            Console.WriteLine("{0} is {1}", player, rname);

            if (rname == Util.DPS.name) {
                dpsCount++;
            } else if (rname == Util.alacDPS.name) {
                boonDPScount++;
                alacCount++;
            } else if (rname == Util.quickDPS.name) {
                boonDPScount++;
                quickCount++;
            } else if (rname == Util.healAlac.name) {
                healCount++;
                alacCount++;
            } else if (rname == Util.healQuick.name) {
                healCount++;
                quickCount++;
            } else {
                Assert.Fail("Unknown role returned");
            }
        }

        Assert.AreEqual(6, dpsCount, "wrong dps count");
        Assert.AreEqual(2, boonDPScount, "Wrong Boondps count");
        Assert.AreEqual(2, healCount, "wrong heal count");
        Assert.AreEqual(2, alacCount, "wrong alac count");
        Assert.AreEqual(2, quickCount, "wrong quick count");

    }

    [TestMethod]
    [Timeout(10000)]
    public void TestUnfilledSquad() {
        var cs = new ConstraintSolver(Util.basicRaidSquad);

        for (int i = 0; i < 9; i++) {
            var playerName = "player" + i.ToString();

            cs.AddPlayerRole(playerName, Util.alacDPS);
            cs.AddPlayerRole(playerName, Util.quickDPS);
            cs.AddPlayerRole(playerName, Util.healAlac);
            cs.AddPlayerRole(playerName, Util.healQuick);
            cs.AddPlayerRole(playerName, Util.DPS);
        }

        Console.WriteLine("get player roles)");
        var roles = cs.GetPlayerRoles();

        Assert.AreEqual(0, roles.Count);

    }

    [TestMethod]
    [Timeout(1000)]
    public void TestBtRequred() {

        // This test needs to be verified that it does test backtracking manually
        // Such as by adding a print statement when it does backracking
        var cs = new ConstraintSolver(Util.basicFractalSquad);

        cs.AddPlayerRole("allP", Util.alacDPS);
        cs.AddPlayerRole("allP", Util.quickDPS);
        cs.AddPlayerRole("allP", Util.healAlac);
        cs.AddPlayerRole("allP", Util.healQuick);
        cs.AddPlayerRole("allP", Util.DPS);

        cs.AddPlayerRole("boonp", Util.healAlac);
        cs.AddPlayerRole("boonp", Util.quickDPS);

        cs.AddPlayerRole("DPS1", Util.DPS);
        cs.AddPlayerRole("DPS1", Util.healQuick);

        cs.AddPlayerRole("DPS2", Util.DPS);

        cs.AddPlayerRole("DPS3", Util.DPS);



        var roles = cs.GetPlayerRoles();

        foreach (var role in roles) {
            Console.WriteLine("{0} as {1}", role.Item1, role.Item2.name);
        }

        Assert.AreEqual(5, roles.Count);
    }

    [TestMethod, Timeout(10000)]
    public void TestRedoSolving() {
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
        var roles2 = cs.GetPlayerRoles();



        Assert.AreEqual(roles.Count(), roles2.Count() );

        for (var i = 0; i < roles.Count; i++) {
            Assert.AreEqual(roles[i].Item1, roles2[i].Item1);
            Assert.AreEqual(roles[i].Item2.name, roles2[i].Item2.name);
        }

    }

    [TestMethod, Timeout(1000)]
    public void TestAddRemove() {
        Console.WriteLine("new cs");
        var cs = new ConstraintSolver(Util.basicFractalSquad);


        Console.WriteLine("add players");
        cs.AddPlayerRole("healPlayer", Util.healAlac);
        cs.AddPlayerRole("healPlayer", Util.healQuick);

        cs.AddPlayerRole("boonPlayer", Util.alacDPS);
        cs.AddPlayerRole("boonPlayer", Util.quickDPS);

        cs.AddPlayerRole("DPSPlayer1", Util.DPS);
        cs.AddPlayerRole("DPSPlayer2", Util.DPS);



        cs.RemovePlayerRole("healPlayer", Util.healAlac);

        cs.RemovePlayer("boonPlayer");

        cs.AddPlayerRole("DPSPlayer3", Util.DPS);

        cs.AddPlayerRole("replacementBoonPlayer", Util.alacDPS);
        cs.AddPlayerRole("replacementBoonPlayer", Util.quickDPS);


        Console.WriteLine("get player roles)");
        var roles = cs.GetPlayerRoles();

        Assert.AreEqual(5, roles.Count);

        foreach (var (player, role) in roles) {
            Console.WriteLine("{0} is {1}", player, role.name);

            if (player == "healPlayer") {
                Assert.IsTrue(role.name == Util.healQuick.name);
            }
            if (player == "boonPlayer") {
                Assert.Fail("boonPlayerWasRemoved");
            }
            if (player == "replacementBoonPlayer") {
                Assert.IsTrue(role.name == Util.alacDPS.name);
            }
            if (player == "DPSPlayer1" ||
                player == "DPSPlayer2" ||
                player == "DPSPlayer3") {
                Assert.IsTrue(role.name == Util.DPS.name);
            }

        }
    }

    [TestMethod, Timeout(1000)]
    public void TestNeededJustDeeps() {
        var cs = new ConstraintSolver(Util.basicFractalSquad);

        cs.AddPlayerRole("healPlayer", Util.healAlac);
        cs.AddPlayerRole("healPlayer", Util.healQuick);

        cs.AddPlayerRole("boonPlayer", Util.alacDPS);
        cs.AddPlayerRole("boonPlayer", Util.quickDPS);

        cs.AddPlayerRole("DPSPlayer1", Util.DPS);
        cs.AddPlayerRole("DPSPlayer2", Util.DPS);
   

        var needed = cs.GetNeededRoles();

        foreach(var (role, count) in needed) {
            Console.WriteLine("role: {0}, count: {1}", role.name, count);
        }

        Assert.AreEqual(needed.Count, 1);
        Assert.AreEqual(needed[0].Item1.name , Util.DPS.name);
        Assert.AreEqual(needed[0].Item2, 1);
    }

    [TestMethod, Timeout(1000)]
    public void TestNeededNearlyEmptySquad() {
        var cs = new ConstraintSolver(Util.basicRaidSquad);
        cs.AddPlayerRole("allP", Util.alacDPS);


        var needed = cs.GetNeededRoles();
        foreach (var (role, count) in needed) {
            Console.WriteLine("role: {0}, count: {1}", role.name, count);
        }

        bool healQuickSeen = false;
        bool dpsSeen = false;
        foreach (var (role, count) in needed) {
         
            if (role.name == Util.healQuick.name) {
                Assert.IsFalse(healQuickSeen);
                healQuickSeen = true;
                Assert.AreEqual(count, 1);
            } else if (role.name == Util.DPS.name) {
                Assert.IsFalse(dpsSeen);
                dpsSeen = true;
                Assert.AreEqual(count, 6);
            } else {
                Assert.Fail("unexpected role returned");
            }

        }

        Assert.IsTrue(dpsSeen);
 
        Assert.IsTrue(healQuickSeen);

    }

    [TestMethod, Timeout(1000)]
    public void TestAvaliable() {
        var cs = new ConstraintSolver(Util.basicFractalSquad);

        cs.AddPlayerRole("allP", Util.alacDPS);
        cs.AddPlayerRole("allP", Util.quickDPS);
        cs.AddPlayerRole("allP", Util.healAlac);
        cs.AddPlayerRole("allP", Util.healQuick);
        cs.AddPlayerRole("allP", Util.DPS);

        cs.AddPlayerRole("DPSPlayer1", Util.DPS);
        cs.AddPlayerRole("DPSPlayer2", Util.DPS);

        var avaliable = cs.GetAvailiableRoles();

        Assert.AreEqual(avaliable.Count, 5);
      

        bool alacdpsSeen = false;
        bool quickdpsSeen = false;
        bool healAlacSeen = false;
        bool healQuickSeen = false;
        bool dpsSeen = false;

        foreach (var (role, count) in avaliable) {
            if (role.name == Util.alacDPS.name) {
                Assert.IsFalse(alacdpsSeen);
                alacdpsSeen = true;
                Assert.AreEqual(count, 1);

            } else if (role.name == Util.quickDPS.name) {
                Assert.IsFalse(quickdpsSeen);
                quickdpsSeen = true;
                Assert.AreEqual(count, 1);
            } else if (role.name == Util.healAlac.name) {
                Assert.IsFalse(healAlacSeen);
                healAlacSeen = true;
                Assert.AreEqual(count, 1);
            } else if (role.name == Util.healQuick.name) {
                Assert.IsFalse(healQuickSeen);
                healQuickSeen = true;
                Assert.AreEqual(count, 1);
            } else if (role.name == Util.DPS.name) {
                Assert.IsFalse(dpsSeen);
                dpsSeen = true;
                Assert.AreEqual(count, 1);
            } else {
                Assert.Fail("unexpected role returned");
            }
        
        }

        Assert.IsTrue(dpsSeen);
        Assert.IsTrue(quickdpsSeen);
        Assert.IsTrue(alacdpsSeen);
        Assert.IsTrue(healAlacSeen);
        Assert.IsTrue(healQuickSeen);


    }
    [TestMethod, Timeout(1000)]
    public void TestAvaliableEmptySquad() {
        var cs = new ConstraintSolver(Util.basicRaidSquad);



        var avaliable = cs.GetAvailiableRoles();

        Assert.AreEqual(avaliable.Count, 5);


        bool alacdpsSeen = false;
        bool quickdpsSeen = false;
        bool healAlacSeen = false;
        bool healQuickSeen = false;
        bool dpsSeen = false;

        foreach (var (role, count) in avaliable) {
            if (role.name == Util.alacDPS.name) {
                Assert.IsFalse(alacdpsSeen);
                alacdpsSeen = true;
                Assert.AreEqual(count, 2);

            } else if (role.name == Util.quickDPS.name) {
                Assert.IsFalse(quickdpsSeen);
                quickdpsSeen = true;
                Assert.AreEqual(count, 2);
            } else if (role.name == Util.healAlac.name) {
                Assert.IsFalse(healAlacSeen);
                healAlacSeen = true;
                Assert.AreEqual(count, 2);
            } else if (role.name == Util.healQuick.name) {
                Assert.IsFalse(healQuickSeen);
                healQuickSeen = true;
                Assert.AreEqual(count, 2);
            } else if (role.name == Util.DPS.name) {
                Assert.IsFalse(dpsSeen);
                dpsSeen = true;
                Assert.AreEqual(count, 6);
            } else {
                Assert.Fail("unexpected role returned");
            }

        }

        Assert.IsTrue(dpsSeen);
        Assert.IsTrue(quickdpsSeen);
        Assert.IsTrue(alacdpsSeen);
        Assert.IsTrue(healAlacSeen);
        Assert.IsTrue(healQuickSeen);


    }
}

