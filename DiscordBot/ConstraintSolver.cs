using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot {
    internal class ConstraintSolver {
        uint nextAttributeIndex = 0;
        Dictionary<String, int> attributeIndexes = new Dictionary<String, int>();

        Dictionary<String, Player> players = new Dictionary<string, Player>();

        ConstraintSolver(Dictionary<string, int> attributeCount, Role[] roles) {
            foreach (var role in roles) {
            }

            foreach (var attribute in attributeCount) {
            }
        }

        struct Role {
            string name;
            String[] attributes;
        }

        struct SquadComp {
            string name;
            Dictionary<String, int> attributeCounts;
        }

        struct Player {
            string name;
            List<Role> roles;
        }

        class DlxLinks {
            string _player;
            string _role;

            DlxLinks _up;
            DlxLinks _down;

            DlxLinks _left;
            DlxLinks _right;
        }
    }
}


