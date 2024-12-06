using Discord;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot {
    internal class DlxColumnHeader : DlxNode {
        public int min;
        public int max;
        public int current;
        public int avail;

        public int Priority() {
            // Lower numbers are higher priority
            int needed = min - current;
            if (needed > 0) {
                return avail - needed;
            } else {
                return int.MaxValue;
            }

        }

        public IEnumerable<DlxColumnHeader> ColumnHeaders() {
            yield return this;
            for (DlxNode n = this.left; n != this; n = n.left) {
                yield return n.columnHeader;
            }

        }

        DlxColumnHeader() : base(0) {
            columnHeader = this;
        }
    }

    internal class DlxNode {
        public DlxNode up;
        public DlxNode down;
        public DlxNode left;
        public DlxNode right;

        public DlxColumnHeader columnHeader;
        public int rowIdx;



        protected DlxNode(int rowIndex) {

            //columnHeader = columnHeader;
            this.rowIdx = rowIndex;
            up = this;
            down = this;
            left = this;
            right = this;
        }

        public DlxNode(DlxColumnHeader columnHeader, int rowIndex) {
            this.columnHeader = columnHeader;
            this.rowIdx = rowIndex;
            up = this;
            down = this;
            left = this;
            right = this;
        }

        public void remove() {
            up.down = down;
            down.up = up;

            left.right = right;
            right.left = left;
        }

        public void reinsert() {
            up.down = this;
            down.up = this;

            left.right = this;
            right.left = this;
        }



        public IEnumerable<DlxNode> OthersInThisRow() {
            for (var item = this.left; item != this; item = this.left) {
                yield return item;
            }
        }

        public IEnumerable<DlxNode> InThisRow() {
            yield return this;
            foreach (var item in OthersInThisRow())
                yield return item;
        }

        public IEnumerable<DlxNode> OthersInThisColumn() {
            for (var item = this.down; item != this; item = this.down) {
                yield return item;
            }
        }

    }

    internal class ReinsertNode {
        protected readonly DlxNode node;

        public ReinsertNode(DlxNode n) {
            node = n;
        }
        public virtual void Reinsert() {
            node.reinsert();
        }
    }

    internal class ReinsertSelectedNode : ReinsertNode {
        public ReinsertSelectedNode(DlxNode n) : base(n) { }

        public override void Reinsert() {
            node.columnHeader.avail++;
            node.columnHeader.current--;
            node.reinsert();
        }
    }

    internal class ReinsertImpossibleNode : ReinsertNode {

        public ReinsertImpossibleNode(DlxNode n) : base(n) { }
        public override void Reinsert() {
            node.columnHeader.avail++;
            node.reinsert();
        }
    }



    internal class DancingLinks {
        Stack<Tuple<DlxNode, Stack<DlxNode>>> backtrackStacks = new Stack<Tuple<DlxNode, Stack<DlxNode>>>();
        List<DlxColumnHeader> columns = new List<DlxColumnHeader>();

        List<int> Solve(DlxColumnHeader hdr) {

            // Chose a cholumn with the fewest choices and count the columns 
            var selectedColumn = hdr;
            int selPrio = selectedColumn.Priority();
            int columnCount = 0;

            foreach (var column in hdr.ColumnHeaders()) {
                columnCount++;
                int colPrio = column.Priority();
                if (selPrio > colPrio) {
                    selectedColumn = column;
                    selPrio = colPrio;
                }
            }

            var rstack = new Stack<ReinsertNode>();

            var remainingColumnHeader = hdr;

            foreach (var rowNode in selectedColumn.OthersInThisColumn()) {
                // Try removing then solve with hdr
                foreach (var node in rowNode.InThisRow()) {
                    var selhdr = node.columnHeader;
                    selhdr.avail--;
                    selhdr.current++;
                    node.remove();
                    rstack.Push(new ReinsertSelectedNode(node));

                    if (selhdr.current == selhdr.max || selhdr.avail == 0) {
                        selhdr.remove();
                        remainingColumnHeader = selhdr.left.columnHeader;

                        rstack.Push(new ReinsertNode(selhdr));

                        foreach (var improw in selhdr.OthersInThisColumn()) {
                            foreach (var impnode in improw.InThisRow()) {
                                var imphdr = impnode.columnHeader;
                                imphdr.avail--;
                                impnode.remove();
                                rstack.Push(new ReinsertImpossibleNode(impnode));

                                if (imphdr.avail + imphdr.current < imphdr.min) {
                                    goto inconsitancyReached;
                                }
                            }
                        }
                    }
                }

                bool allColumnsDone = true;
                foreach (var colhdr in remainingColumnHeader.ColumnHeaders()) {
                    if (colhdr.min < colhdr.current) {
                        allColumnsDone = false;
                    }

                }

                if (allColumnsDone) {
                    return new List<int> { rowNode.rowIdx };
                } else {
                    var restOfSolution = Solve(remainingColumnHeader);
                    if (restOfSolution.Count() != 0) {
                        restOfSolution.Append(rowNode.rowIdx);
                        return restOfSolution;
                    } else {
                        goto inconsitancyReached;
                    }
               
                }

                // Failed so reinsert node and choose something else
                inconsitancyReached:
                while (rstack.Count() != 0) {
                    var reinsertNode = rstack.Pop();
                    reinsertNode.Reinsert();
                }

            }
            // return empty to signal we cant choose any here so next row up must choose
            return new List<int>();

        }

        List<int> Solve() {
            // Go along coluimn headers find min of (min - curr) - avail for positive min-curr
            DlxColumnHeader selectedColumn = columns[0];
            return Solve(columns[0]);
        }


    }
}
