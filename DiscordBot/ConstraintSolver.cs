using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DiscordBot;

public struct SquadRequirements {
    public string name; // Used for lookup by user for appoprate role set (raid wind / fractal comp)
    public uint teamSize;
    public List<Role> roles; // The roles that are valid
    public Dictionary<string, int> attributeCount; // How much of heal/boons/dps are needed in total
};
public struct Role {
    public string name;
    public string[] attributes;
}
public struct Player {
    public string name;
    public Role[] Roles;
}
public class Util {
    public static Role healAlac = new Role() {
        name = "HealAlac",
        attributes = new string[] { "heal", "alacrity" }
    };
    public static Role healQuick = new Role() {
        name = "HealQuick",
        attributes = new string[] { "heal", "quickness" }
    };
    public static Role alacDPS = new Role() {
        name = "AlacDPS",
        attributes = new string[] { "boondps", "alacrity" }
    };
    public static Role quickDPS = new Role() {
        name = "QuickDPS",
        attributes = new string[] { "boondps", "quickness" }
    };
    public static Role DPS = new Role() {
        name = "DPS",
        attributes = new string[] { "DPS" }
    };


    public static SquadRequirements basicRaidSquad = new SquadRequirements() {
        name = "standard raid squad",
        teamSize = 10,
        roles = new List<Role>() { healAlac, healQuick, alacDPS, quickDPS, DPS },
        attributeCount = new Dictionary<string, int>() {
            { "heal", 2 },
            { "boondps", 2 },
            { "alacrity", 2 },
            { "quickness", 2 },
            { "DPS", 6 }
        },
    };
    public static SquadRequirements basicFractalSquad = new SquadRequirements() {
        name = "standard fractal",
        teamSize = 5,
        roles = new List<Role>() { healAlac, healQuick, alacDPS, quickDPS, DPS },
        attributeCount = new Dictionary<string, int>() {
            { "heal", 1 },
            { "boondps", 1 },
            { "alacrity", 1 },
            { "quickness", 1 },
            { "DPS", 3 }
        },
    };
}


public class ConstraintSolver {
    // Converts input contstraints to DancingLinks algorithm

    DancingLinks Dlx = new DancingLinks();

    Dictionary<string, int> AttributeColumnHeaderIdxs = new Dictionary<string, int>();

    Dictionary<string, Player> Players = new Dictionary<string, Player>();
    Dictionary<int, (string,Role)> DlxRowIdxMap = new Dictionary<int , (string,Role)>();

    class Player {
        public string Name;
        public int ColumnHeaderIdx;
        public Dictionary<Role, int> Roles;

        public Player(string name, int columnHeaderIdx) {
            Name = name;
            ColumnHeaderIdx = columnHeaderIdx;
            Roles = new Dictionary<Role, int>();
        }
    }

    SquadRequirements Requirements;

    public ConstraintSolver(SquadRequirements req) {
        Requirements = req;

        foreach (var (attr, count) in req.attributeCount) {
            // Insert column header to dlx

            var attridx = Dlx.addColumnHeader(count, count, attr);
            

            AttributeColumnHeaderIdxs[attr] = attridx;
        }

    }

    public int AddPlayerRole(string playername, Role role) {



        Player player;
        if (! Players.TryGetValue(playername, out player) ) {
            var headerIdx = Dlx.addColumnHeader(1, 1, playername + "_hdr");
            player = new Player(playername, headerIdx);
            Players[playername] = player;
        }

        if (player.Roles.ContainsKey(role)) {
            throw new Exception("duplicate roles");
        }

        var rowIdx = Dlx.getNextRowIdx();
        player.Roles.Add(role, rowIdx);

        foreach (var attr in role.attributes) {
            // Insert nodes
            var attridx = AttributeColumnHeaderIdxs[attr];

            // Lookup idx;
            Dlx.addNode(rowIdx, attridx, playername + " as " + role.name + " attr : " + attr);
        }

        // insert player node (and column header if needed)
        Dlx.addNode(rowIdx, player.ColumnHeaderIdx, playername + " as " + role.name + " playerCol");

        DlxRowIdxMap.Add(rowIdx, (playername ,role) );

        return rowIdx;
    }

    // Returns the number of remaining roles of the player, removing the player if it reaches 0
    public int RemovePlayerRole(string playername, Role role) {
        Player player;

        if (!Players.TryGetValue(playername, out player)) {
            throw new Exception("No such player: " + playername);
        }

        if (player.Roles.ContainsKey(role)) {

            //remove role node
            var dlxRowIdx = player.Roles[role];

            player.Roles.Remove(role);
            Dlx.removeRow(dlxRowIdx);
            DlxRowIdxMap.Remove(dlxRowIdx);

            var count = player.Roles.Count();

            if (count == 0) {
                //remove player
                Players.Remove(playername);
                Dlx.removeColumnHeader(player.ColumnHeaderIdx);
            }

            return count;
        } else {
            throw new Exception(playername + " is has no such role: " + role.name);
        }
    }

    public void RemovePlayer(string playername) {
        Player player;

        if (!Players.TryGetValue(playername, out player)) {
            throw new Exception("No such player: " + playername);
        }

        foreach (var role in player.Roles.Keys) {
            RemovePlayerRole(playername, role);
        }
    }

    // Minimums of each role
    public List<(Role, int)> GetNeededRoles() {
        var playersToAdd = this.Requirements.teamSize - this.Players.Count();

        var playerNames = new List<string>();

        for (var i = 0; i < playersToAdd; i++) {
            playerNames.Add("!£$^&,>IA " + i.ToString());
        }

        var neededRoles = new List<(Role, int)>();

        foreach (var role in Requirements.roles) {
            // For each role try to make a valid squad with s=0..n players as selected role
            // and n-s players that can fill that role


            // Try to fill a squad with playersToAdd - i of role 
            //                   and     i  of anything but role
            // record last Success
            int iWithSuccesWithLeastOfRole = -1;
            for (var i = 0; i <= playersToAdd; i++) {

                for (var j = 0; j < i; j++) {
                    foreach (var otherRoles in Requirements.roles) {
                        if (otherRoles.name != role.name) {
                            AddPlayerRole(playerNames[j], otherRoles);
                        }
                    }
                }
                for (var j = i; j < playersToAdd; j++) {
                    AddPlayerRole(playerNames[j], role);
                }

                var squad = GetPlayerRoles();

                foreach (var player in playerNames) {
                    RemovePlayer(player);
                }

                if (squad.Count != 0) {
                    iWithSuccesWithLeastOfRole = i;
                }
            }
            if (iWithSuccesWithLeastOfRole == -1) {
                throw new Exception("Needed roles called on impossible squad");
            }
            int needed = (int)(playersToAdd - iWithSuccesWithLeastOfRole);
            if (needed != 0) {
                neededRoles.Add((role, needed));
            }
        }

        return neededRoles;  
    }

    // Maximums of each role
    public List<(Role, int)> GetAvailiableRoles() {
        var playersToAdd = this.Requirements.teamSize - this.Players.Count();

        var playerNames = new List<string>();

        for (var i = 0; i < playersToAdd; i++) {
            playerNames.Add("!£$^&,>IA " + i.ToString());
        }

        var avaliableRoles = new List<(Role, int)>();

        foreach (var role in Requirements.roles) {
      
            // Try to fill a squad with i of role 
            //                   and    playersToAdd - i of anything but role
            // record last Success
            int iWithSuccesWithMostOfRole = -1;
            for (var i = 0; i <= playersToAdd; i++) {

                for (var j = 0; j < i; j++) {
                    AddPlayerRole(playerNames[j], role);
                }

                for (var j = i; j < playersToAdd; j++) {
                    foreach (var otherRoles in Requirements.roles) {
                        if (otherRoles.name != role.name) {
                            AddPlayerRole(playerNames[j], otherRoles);
                        }
                    }
                }

                var squad = GetPlayerRoles();

                foreach (var player in playerNames) {
                    RemovePlayer(player);
                }

                if (squad.Count != 0) {
                    iWithSuccesWithMostOfRole = i;
                }
            }
            if (iWithSuccesWithMostOfRole == -1) {
                throw new Exception("Avaliable roles called on impossible squad");
            }

            int avaliable = (int)(iWithSuccesWithMostOfRole);
            if (avaliable != 0) {
                avaliableRoles.Add((role, avaliable));
            }
        }

        return avaliableRoles;
    }

    // Solve for players
    public List<(string, Role)> GetPlayerRoles() {
        var dlxRes = Dlx.Solve();


        if (dlxRes.Count > 0) {
            CheckResults(dlxRes);
        }

        var res = new List<(string, Role)>();

        foreach (var role in dlxRes) {
            res.Add(DlxRowIdxMap[role]);
        }

        return res;
    }

    private void CheckResults(List<int> dlxRes) {

        Dictionary<string, int> players = new Dictionary<string, int>();
        Dictionary<string, int> attributes = new Dictionary<string, int>();

        foreach (var player in Players.Keys ) {
            players.Add(player, 0);
        }

        foreach (var attrib in Requirements.attributeCount) {
            attributes.Add(attrib.Key, 0);
        }

        // Each player once
        foreach (var rolidx in dlxRes) {
            var (playername, role) = DlxRowIdxMap[rolidx];

            players[playername] = players[playername] + 1;
          
            foreach (var attr in role.attributes) {
                attributes[attr]++;
            }           
        }

        
        List<string> errors = new List<string>();

        foreach (var player in players) {
            if (player.Value != 1) {
                
                errors.Add(player.Key + ": " + player.Value);
            }
        }

        foreach (var attr in Requirements.attributeCount) {
            if (attr.Value != attributes[attr.Key]) {
                errors.Add("Incorrect " + attr.Key + " - expected: " + attr.Value + " got:" +  attributes[attr.Key]);
            }
        }

        if (errors.Count > 0) {
            throw new Exception(String.Join('\n', errors));
        }
    }
}


