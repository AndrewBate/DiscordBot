using Discord;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DiscordBot;
internal class DlxColumnHeader : DlxNode {
    public int min;
    public int max;
    public int current = 0;
    public int avail = 0;

    public int colIdx;

    //public override bool isColumnHeader() { return true; }

    public IEnumerable<DlxColumnHeader> ColumnHeaders() {
        yield return this;
        for (DlxNode n = this.left; n != this; n = n.left) {
            yield return n.columnHeader;
        }

    }

    public DlxColumnHeader(int min, int max, int colIdx, string nodeinfo) : base(-1, nodeinfo) {
        columnHeader = this;
        this.min = min;
        this.max = max;
        this.colIdx = colIdx;
    }
}

public enum RemoveMode {
    None,
    Plain,
    Selected,
    Impossible
}


internal class DlxNode {
    public DlxNode up;
    public DlxNode down;
    public DlxNode left;
    public DlxNode right;

    public DlxColumnHeader columnHeader;
    public int rowIdx;
    public string nodeInfo;
    public RemoveMode mode = RemoveMode.None;





    protected DlxNode(int rowIndex, string nodeInfo) {

        //columnHeader = columnHeader;
        this.rowIdx = rowIndex;
        this.nodeInfo = nodeInfo;
        up = this;
        down = this;
        left = this;
        right = this;

    }

    public DlxNode(DlxColumnHeader columnHeader, int rowIndex, string nodeInfo) {
        this.columnHeader = columnHeader;
        this.rowIdx = rowIndex;
        this.nodeInfo = nodeInfo;
        up = this;
        down = this;
        left = this;
        right = this;
    }

    public bool isColumnHeader() { return rowIdx < 0; }

    public void remove(RemoveMode m) {
        up.down = down;
        down.up = up;

        left.right = right;
        right.left = left;


        switch (mode) {
            case RemoveMode.None:
                break;
            default:
                throw new Exception("removing already removed");
        }

        mode = m;

        switch (m) {
            case RemoveMode.Selected:
                if (isColumnHeader()) { throw new Exception("selecting column header"); }
                columnHeader.avail--;
                columnHeader.current++;
                break;

            case RemoveMode.Impossible:
                if (isColumnHeader()) { throw new Exception("marking column header as impossible"); }
                columnHeader.avail--;
                break;

            case RemoveMode.Plain:
                if (!isColumnHeader()) { throw new Exception("removing node withoug updating header"); }
                break;

            default:
                break;
        }


    }

    public void reinsert() {
        up.down = this;
        down.up = this;

        left.right = this;
        right.left = this;

        switch (mode) {
            case RemoveMode.Plain:
                break;

            case RemoveMode.Selected:
                columnHeader.avail++;
                columnHeader.current--;
                break;

            case RemoveMode.Impossible:
                columnHeader.avail++;
                break;


            case RemoveMode.None: throw new Exception();

        }
        mode = RemoveMode.None;
    }

}



internal class DancingLinks {
    List<DlxColumnHeader> columns = new List<DlxColumnHeader>();
    int nextRowIdx = 0;
    List<DlxNode?> rowStarts = new List<DlxNode?>();

    Stack<Stack<DlxNode>> btState = new Stack<Stack<DlxNode>>();

    int recursionDepth = 0;


    public int getNextRowIdx() {
        rowStarts.Add(null);
        return nextRowIdx++;
    }

    public void addNode(int rowIdx, int columnIdx, string nodeInfo) {
        var node = new DlxNode(columns[columnIdx], rowIdx, nodeInfo);
        node.columnHeader.avail++;

        node.up = node.columnHeader.up;
        node.down = node.columnHeader;

        node.up.down = node;
        node.down.up = node;

        var first = rowStarts[rowIdx];


        if (first == null) {
            rowStarts[rowIdx] = node;
        } else {
            node.left = first.right;
            node.right = first;

            node.left.right = node;
            node.right.left = node;
        }
    }

    public int addColumnHeader(int min, int max, string nodeInfo) {
        var hdr = new DlxColumnHeader(min, max, columns.Count(), nodeInfo);
        columns.Add(hdr);

        if (columns.Count() != 0) {
            var first = columns[0];
            hdr.left = first.left;
            hdr.right = first;

            first.left.right = hdr;
            first.left = hdr;
        }

        return columns.Count() - 1;
    }

    List<int> Solve(DlxColumnHeader hdr) {
        // Chose a cholumn with the fewest choices and count the columns 
        var selectedColumn = hdr;
        // selPrio = number of spare options;
        int selPrio = int.MaxValue;
        int columnCount = 0;

        foreach (var column in hdr.ColumnHeaders()) {
            columnCount++;

            int needed = column.min - column.current;

            if (needed > 0) {

                int prio = column.avail - needed;

                if (prio < selPrio) {
                    selectedColumn = column;
                }

                if (prio < 0) {
                    return new List<int>();
                }

            } 
        }

        var rstack = new Stack<DlxNode>();
        btState.Push(rstack);

        // First node in this row, will be removed during traversal
        // This is a backtracking point
        for (var rowNode = selectedColumn.down; rowNode != selectedColumn; rowNode = rowNode.down) {
            Debug.Assert(!rowNode.isColumnHeader());
            // Try removing then solve with hdr

            // Remove all the nodes in this row
            for (var node = rowNode; node.mode == RemoveMode.None; node = node.left) {
                Debug.Assert(!node.isColumnHeader());
                var selhdr = node.columnHeader;

                node.remove(RemoveMode.Selected);
                rstack.Push(node);

                if (selhdr.current == selhdr.max || selhdr.avail == 0) {
                    selhdr.remove(RemoveMode.Plain);
                    rstack.Push(selhdr);

                    // Remove all rows that satisfy this filled column
                    for (var improw = selhdr.down; improw.mode == RemoveMode.None; improw = improw.down) {
                        // Remove all nodes in this row
                        for (var impnode = improw; impnode.mode == RemoveMode.None; impnode = impnode.left) {

                            var imphdr = impnode.columnHeader;

                            impnode.remove(RemoveMode.Impossible);
                            rstack.Push(impnode);

                            // Backtrack if this makes unsatisfyable constraints
                            if (imphdr.avail + imphdr.current < imphdr.min) {
                                goto inconsitancyReached;
                            }

                            // Remove empty column header in situations where min != max
                            if (imphdr.avail == 0 && imphdr != selhdr) {
                                imphdr.remove(RemoveMode.Plain);
                                rstack.Push(imphdr);
                            }
                        }
                    }
                }
            }

            DlxColumnHeader? remainingColumnHeader = null;

            foreach (var colhdr in columns) {
                if (colhdr.mode == RemoveMode.None) {

                    if (colhdr.min > colhdr.current) {
                        remainingColumnHeader = colhdr;
                    }
                }

            }

            if (remainingColumnHeader == null) {
                return new List<int> { rowNode.rowIdx };
            } else {
                recursionDepth++;
                if (recursionDepth > columns.Count() * 10) {
                    throw new Exception("too much recursion");
                }
                var restOfSolution = Solve(remainingColumnHeader);
                recursionDepth--;
                if (restOfSolution.Count() != 0) {
                    restOfSolution.Add(rowNode.rowIdx);
                    return restOfSolution;
                } else {
                    goto inconsitancyReached;
                }

            }

        // Failed so reinsert node and choose something else
        inconsitancyReached:
            
            while (rstack.Count() != 0) {
                var reinsertNode = rstack.Pop();
                reinsertNode.reinsert();
            }

        }
        btState.Pop();

        // return empty to signal we cant choose any here so next row up must choose
        return new List<int>();
    }

    public List<int> Solve() {
        recursionDepth = 0;

        DlxColumnHeader selectedColumn = columns[0];
        var res = Solve(columns[0]);

        while (btState.Count() != 0) {
            var substack = btState.Pop();
            while (substack.Count() != 0) {
                substack.Pop().reinsert();
            }
        }
        return res;
    }


}

