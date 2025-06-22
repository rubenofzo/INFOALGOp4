namespace Dungeon;
using Xunit;
using System.Collections.Generic;
using System.Linq;

public class tests
{
    // ========== STEP 1: INPUT PARSING ==========
    [Fact]
    public void TestParseInput_Simple()
    {
        string[] lines = {
            "4 4 2",
            "1 2",
            "2 3",
            "3 4",
            "4 2",
            "2 4"
        };

        var (n, m, s, edges, coins) = Parser.ParseInput(lines);
        Assert.Equal(4, n);
        Assert.Equal(4, m);
        Assert.Equal(2, s);
        Assert.Equal((1, 2), edges[0]);
        Assert.Contains(4, coins);
    }

    // ========== STEP 2: SCC DETECTION ==========
    [Fact]
    public void TestTarjanSCC_SimpleCycle()
    {
        var graph = new Dictionary<int, List<int>>
        {
            [1] = new List<int>{2},
            [2] = new List<int>{3},
            [3] = new List<int>{1}
        };

        var sccs = SCCFinder.FindSCCs(graph, 3);
        Assert.Single(sccs);
        Assert.Contains(1, sccs[0]);
    }

    [Fact]
    public void TestTarjanSCC_MultipleSCCs()
    {
        var graph = new Dictionary<int, List<int>>
        {
            [1] = new List<int>{2},
            [2] = new List<int>(),
            [3] = new List<int>{4},
            [4] = new List<int>{3}
        };

        var sccs = SCCFinder.FindSCCs(graph, 4);
        Assert.Equal(3, sccs.Count); // [1,2], [3,4] are separate SCCs
    }

    // ========== STEP 3: DAG CONDENSATION ==========
    [Fact]
    public void TestCondensation_SimpleGraph()
    {
        var graph = new Dictionary<int, List<int>>
        {
            [1] = new List<int>{2},
            [2] = new List<int>{3},
            [3] = new List<int>{4}
        };

        var sccResult = SCCFinder.FindSCCs(graph, 4);
        var (dag, sccMap) = Condenser.CondenseGraph(graph, sccResult);
        Assert.Equal(3, dag.Count); // should be 3 SCC nodes
    }

    // ========== STEP 4: DAG DP ==========
    [Fact]
    public void TestMaxCoinsDP_Simple()
    {
        // DAG: 0 → 1 → 2
        var dag = new Dictionary<int, List<int>>
        {
            [0] = new List<int>{1},
            [1] = new List<int>{2},
            [2] = new List<int>()
        };
        var coinCount = new Dictionary<int, int> { [0] = 1, [1] = 1, [2] = 2 };
        var result = DAGDP.GetMaxCoins(dag, coinCount, 0);
        Assert.Equal(4, result.maxCoins);
    }

    // ========== STEP 5: PATH RECONSTRUCTION ==========
    [Fact]
    public void TestReconstructCoinPath_Simple()
    {
        var parent = new Dictionary<int, int?> { [2] = 1, [1] = 0, [0] = null };
        var roomPerScc = new Dictionary<int, List<int>>
        {
            [0] = new List<int>{1},
            [1] = new List<int>{2},
            [2] = new List<int>{4}
        };

        var path = PathReconstructor.Reconstruct(parent, roomPerScc, 2);
        Assert.Equal(new List<int>{1, 2, 4}, path);
    }

    // ========== STEP 6: FULL EXAMPLE ==========
    [Fact]
    public void TestFullFlow_ExampleFromAssignment()
    {
        string[] lines = {
            "12 16 9",
            "1 4", "1 5", "2 3", "2 1", "3 2", "4 3",
            "5 7", "5 6", "6 8", "8 12", "7 9", "9 7",
            "9 11", "9 10", "10 8", "10 7",
            "2 3 10 6 5 8 4 11 9"
        };

        var result = DungeonSolver.Solve(lines);
        Assert.Equal(7, result.count);
        Assert.True(result.coins.SequenceEqual(new List<int>{4, 2, 3, 5, 9, 10, 8})
                 || result.coins.SequenceEqual(new List<int>{2, 3, 4, 5, 9, 10, 8}));
    }
}