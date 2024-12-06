using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot {

    struct SquadRequirements {
        public string name; // Used for lookup by user for appoprate role set (raid wind / fractal comp)
        public List<Role> roles; // The roles that are valid
        public Dictionary<string, int> attributeCount; // How much of heal/boons/dps are needed in total
    };
    struct Role {
        public string name;
        public string[] attributes;
    }
    struct Player {
        public string name;
        public Role[] Roles;
    }
    class util {
        public static SquadRequirements basicRaidSquad = new SquadRequirements() {
            name = "standard raid squad",
            roles = new List<Role>() {
                new Role() {
                     name = "HealAlac",
                     attributes = new string[] {"heal", "alacrity"}
                },
                new Role() {
                    name = "HealQuick",
                    attributes = new string[] {"heal", "quickness"}
                },   
                new Role {
                    name = "AlacDPS",
                    attributes = new string[] {"boondps", "alacrity"}
                },
                new Role {
                    name = "QuickDPS",
                    attributes = new string[] {"boondps", "quickness"}
                },
                new Role {
                    name = "DPS",
                    attributes = new string[] {"DPS"}
                }

            },
            attributeCount = new Dictionary<string, int>() {
                { "heal", 2 },
                { "boondps", 2 },
                { "alacrity", 2 },
                { "quickness", 2 },
                { "DPS", 6 }
            },
        };
    }


    internal class ConstraintSolver {
        // Converts input contstraints to DancingLinks algorithm


    }
}


