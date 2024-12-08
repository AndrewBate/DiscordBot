using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DiscordBot {

    public struct SquadRequirements {
        public string name; // Used for lookup by user for appoprate role set (raid wind / fractal comp)
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
            name = "standard raid squad",
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

        Dictionary<string, int> AttributeColumnHeaderIdxs =
            new Dictionary<string, int>();  

        Dictionary<string, int> PlayerColumnHeaderIdxs =
            new Dictionary<string, int>();

        List<Tuple<string, Role>> PlayerRoles
            = new List<Tuple<string, Role>>() { new Tuple<string, Role>("ErrorPlayer", Util.DPS) };

        public ConstraintSolver(SquadRequirements req) {
            
            foreach (var (attr, count) in req.attributeCount) {
                // Insert column header to dlx

               
                var attridx = Dlx.addColumnHeader(count, count);
                Console.WriteLine("attr {0}, idx = {1} ", attr, attridx);

               AttributeColumnHeaderIdxs[attr] = attridx;
            }
            
        }

        public int AddPlayerRole(string playername, Role role) {
            Console.WriteLine("add player: {0} as {1}", playername, role.name);
            var rowIdx = Dlx.getNextRowIdx();

            foreach(var attr in role.attributes) {
                // Insert nodes
                var attridx = AttributeColumnHeaderIdxs[attr];

                // Lookup idx;
                Dlx.addNode(rowIdx, attridx);
            }

            // Lookup player column idx
            int playeridx;

            if (PlayerColumnHeaderIdxs.ContainsKey(playername)) {
                playeridx = PlayerColumnHeaderIdxs[playername];
            } else {
                playeridx = Dlx.addColumnHeader(1, 1);
                PlayerColumnHeaderIdxs[playername] = playeridx;
            }

            // insert player node (and column header if needed)
            Dlx.addNode(rowIdx, playeridx);

            PlayerRoles.Add(new Tuple<string, Role>(playername, role));

            return rowIdx;
        }

        public int RemovePlayerRole(string playername, Role role) {
            throw new NotImplementedException();
        }

        public int RemovePlayer(string playername) {
            throw new NotImplementedException();
        }

        // Minimums of each role
        public List<Tuple<Role, int>> GetNeededRoles() {
            return new List<Tuple<Role, int>>();
        }

        // Maximums of each role
        public List<Tuple<Role, int>> GetAvailiableRoles() {
            return new List<Tuple<Role, int>>();
        }

        // Solve for players
        public List<Tuple<string, Role>> GetPlayerRoles() {
            var dlxRes = Dlx.Solve();

            Console.WriteLine("final size = {0}", dlxRes.Count());

            

            var res = new List<Tuple<string, Role>>();

            foreach (var role in dlxRes) {
                res.Add(PlayerRoles[role]);
            }

            return res;
        }
    }
}


