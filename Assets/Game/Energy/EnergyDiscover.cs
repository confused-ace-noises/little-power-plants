using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class DiscoverEnergySystem : MonoBehaviour
{
    // [SerializeField] private EnergyTowerBehavior towerBehavior;
    public World world;

    public static Action<HashSet<Vector2Int>, BuildingBehavior> registerSingle;
    public static Action<HashSet<Vector2Int>, HashSet<Vector2Int>, BuildingBehavior> connectNew;
    public static Action<HashSet<Vector2Int>, HashSet<Vector2Int>> connectExisting;

    Dictionary<HashSet<Vector2Int>, EnergyNode> nodes;

    List<Network> networks;

    void Awake()
    {
        nodes = new Dictionary<HashSet<Vector2Int>, EnergyNode>(HashSet<Vector2Int>.CreateSetComparer());
        networks = new List<Network>();

        // EnergyTowerBehavior.newConnection += ReactToNewConnections;
        registerSingle += RegisterSingle;
        connectNew += Connect;
        connectExisting += ConnectExisting;
        BuildingPlacementSystem.BuildingGotPlaced += RegisterFromBuildingPlaced;
        BuildingPlacementSystem.BuildingGotRemoved += RemoveNodeFromEvent;
    }

    void Start()
    {
        Debug.Log(Money.Instance);
        Money.Instance.NewAdder(CalculateMoney);
    }

    float CalculateMoney()
    {
        string dbgPrint = "";

        foreach (Network netwrok in networks)
        {
            dbgPrint += netwrok.PrintDebug() + "\n ----------------------------- \n";
        }
        
        Debug.Log(dbgPrint);

        float totalReturn = 0;

        foreach (Network network in networks)
        {
            float production = network.productionWithoutSink;
            float net = network.netValue;

            if (production <= 0) continue;
            if (net >= 0) continue; // no sink drawing

            // sink draws -net, but can't earn more than what production makes
            totalReturn += Mathf.Min(-net, production);
        }

        return totalReturn;
    }

    private void RegisterFromBuildingPlaced(Building building, GameObject GO, HashSet<Vector2Int> family)
    {
        if (building.buildingBehavior is EnergyTowerBehavior) {
            Debug.Log("RegisterFromBuildingPlaced is skipping registering a power tower");
            return;
        } // they handle it themselves  
        Debug.Log("Registered Auto");
        RegisterSingle(family, building.buildingBehavior);
    }

    void RegisterSingle(HashSet<Vector2Int> places, BuildingBehavior behavior)
    {
        Debug.Log("Registered manual");
        bool exists = nodes.TryGetValue(places, out _);
        if (exists) return;
        Debug.Log("Registered manual didn't already exist");

        var node = new EnergyNode(places, new HashSet<EnergyNode>(), behavior.EnergyProduction());
        NewNode(node);
    }

    void Connect(HashSet<Vector2Int> connectedTo, HashSet<Vector2Int> connected, BuildingBehavior connectedBehavior)
    {
        Debug.Log("Connect new");
        bool exists = nodes.TryGetValue(connectedTo, out EnergyNode node);
        
        if (!exists) {
            Debug.LogError("tried to access nonexistant node during connection - expect problems"); 
            return;
        }

        Debug.Log("Connect new passed check");
        var newNode = new EnergyNode(connected, new HashSet<EnergyNode>() {node}, connectedBehavior.EnergyProduction());
        NewNode(newNode);
        DebugLog();
    }

    void ConnectExisting(HashSet<Vector2Int> one, HashSet<Vector2Int> two)
    {
        bool exists1 = nodes.TryGetValue(one, out EnergyNode node1);
        
        if (!exists1) {
            Debug.LogError("tried to access nonexistant node during existing node connection - expect problems"); 
            return;
        }

        bool exists2 = nodes.TryGetValue(two, out EnergyNode node2);
        
        if (!exists2) {
            Debug.LogError("tried to access nonexistant node during existing node connection - expect problems"); 
            return;
        }

        BridgeNodes(node1, node2);
        DebugLog();
    }

    private void RemoveNodeFromEvent(Building _, HashSet<Vector2Int> position)
    {
        var exists = nodes.TryGetValue(position, out EnergyNode node);

        if (!exists)
        {
            Debug.LogError("tried to remove a node which wasn't registered. how?");
            return;
        }

        RemoveNode(node);
    }

    public void RemoveNode(EnergyNode nodeToRemove)
    {
        #nullable enable
        Network? owningNetwork = GetNetworkOfNode(nodeToRemove);

        if (owningNetwork == null) {
            Debug.LogError("Tried to remove a node not belonging to a network, maybe this is fine?");
            nodes.Remove(nodeToRemove.position);
            return;
        };

        nodes.Remove(nodeToRemove.position);

        // Sever all connections 
        foreach (EnergyNode neighbor in nodeToRemove.connections)
        {
            neighbor.connections.Remove(nodeToRemove);
        }

        networks.Remove(owningNetwork);

        // re-create networks from neighbours

        HashSet<EnergyNode> visited = new HashSet<EnergyNode>();

        foreach (EnergyNode neighbor in nodeToRemove.connections)
        {
            if (visited.Contains(neighbor)) continue;

            // bfs to find all nodes reachable from this neighbor
            HashSet<EnergyNode> component = new HashSet<EnergyNode>();
            Queue<EnergyNode> queue = new Queue<EnergyNode>();
            queue.Enqueue(neighbor);

            while (queue.Count > 0)
            {
                EnergyNode current = queue.Dequeue();
                if (!component.Add(current)) continue;

                foreach (EnergyNode connected in current.connections)
                    queue.Enqueue(connected);
            }

            // build new network
            var networkNodes = new Dictionary<HashSet<Vector2Int>, EnergyNode>();
            foreach (EnergyNode n in component)
                networkNodes[n.position] = n;

            networks.Add(new Network(networkNodes));
            visited.UnionWith(component);
        }
    }

    private float? GetValueOfCell(Vector2Int cell)
    {
        if (world.worldTiles.LoadedTileDict.TryGetValue(cell, out Tile tile))
        {
            var occ = tile.Occupier();

            if (occ != null)
            {
                var b = occ.Value.building;

                return b.buildingBehavior.EnergyProduction();
            }
        }
        
        return null;
    }

    private void DebugLog()
    {
        string dbgPrint = "";

        foreach (Network netwrok in networks)
        {
            dbgPrint += netwrok.PrintDebug() + "\n ----------------------------- \n";
        }
        
        Debug.Log(dbgPrint);
    }

    #nullable enable
    private Network? GetNetworkOfNode(EnergyNode node) => node.owningNetwork;

    public void NewNode(EnergyNode node)
    {
        nodes.Add(node.position, node);

        #nullable enable
        Network? connectedNetwork = null;

        int needsBridging = 0;

        foreach (Network network in networks)
        {
            bool bridgeCheck = false;
            foreach (EnergyNode connected in network.networkNodes.Values.Where(conn => node.connections.Contains(conn)))
            {
                // tried to get touching edges detection, but it would lead to so many
                // problems i honestly just give up, player will just have to connect 
                // stuff themselves
                
                // .Where(conn => node.connections.Contains(conn))
                //    bool edgesTouching = connected.IsInContact(node);

                // if (!edgesTouching && !node.connections.Contains(connected)) continue;

                if (!bridgeCheck) 
                {
                    needsBridging += 1;
                    bridgeCheck = true;
                }

                connectedNetwork = network;
                // this is kinda sketchy but eh
                connected.connections.Add(node);
                
                // if (edgesTouching) node.connections.Add(connected);
            }
        }

        if (connectedNetwork != null)
        {
            // existing network was found
            connectedNetwork.AddNode(node);

            if (needsBridging != 1)
            {
                // TODO: handle network bridging
                Debug.LogError("Still havent implemented network briding, will break");
            }
        } else
        {
            // no existing network was found, make new
            Network network = new Network();
            network.AddNode(node);       
            networks.Add(network);
        }
    }

    /// <summary>
    /// this operation is commutative
    /// </summary>
    /// <param name="node1"></param>
    /// <param name="node2"></param>
    public void BridgeNodes(EnergyNode node1, EnergyNode node2)
    {
        Network network1 = GetNetworkOfNode(node1) ?? throw new NullReferenceException("BridgeNodes: nodes should belong to a Network when attempting to bridge");
        Network network2 = GetNetworkOfNode(node2) ?? throw new NullReferenceException("BridgeNodes: nodes should belong to a Network when attempting to bridge");
        // if they're on the same network, do nothing
        if (network1 != network2)
        {
            network1.MergeNetwork(network2, node1, node2);
            networks.Remove(network2);
        }
    }
}

public class Network
{
    public Network()
    {
        _networkNodes = new Dictionary<HashSet<Vector2Int>, EnergyNode>();
        networkNodes = new(_networkNodes);
    }

    public Network(Dictionary<HashSet<Vector2Int>, EnergyNode> nodes)
    {
        _networkNodes = new Dictionary<HashSet<Vector2Int>, EnergyNode>();

        foreach (var (key, node) in nodes)
        {
            node.owningNetwork = this;
            this._networkNodes[key] = node;
        }

        networkNodes = new(_networkNodes);
    }

    public float netValue => _networkNodes.Values.Sum(n => n.production);
    public float productionWithoutSink => _networkNodes.Values
        .Where(n => n.production > 0)
        .Sum(n => n.production);

    private Dictionary<HashSet<Vector2Int>, EnergyNode> _networkNodes;
    public ReadOnlyDictionary<HashSet<Vector2Int>, EnergyNode> networkNodes;

    public void AddNode(EnergyNode node)
    {
        node.owningNetwork = this;
        _networkNodes.Add(node.position, node);
    }

    public void MergeNetwork(Network network, EnergyNode node1, EnergyNode node2)
    {
        foreach (var (key, node) in network._networkNodes)
        {
            node.owningNetwork = this;
            this._networkNodes[key] = node;
        }

        _networkNodes[node1.position].connections.Add(node2);
        _networkNodes[node2.position].connections.Add(node1);
    }

    public string PrintDebug()
    {
        string dbgString = "";
        foreach (var node in _networkNodes)
            dbgString += node.Value.GetString();
        return dbgString;
    }
}

public class EnergyNode
{
    public EnergyNode(HashSet<Vector2Int> position, HashSet<EnergyNode> connections, float production)
    {
        this.connections = connections;
        this.position = position;
        this.production = production;
    }
    public HashSet<EnergyNode> connections;
    public HashSet<Vector2Int> position;
    public Network? owningNetwork;
    public float production; // can be negative

    private string GetSimpleString()
    {
        return $"\t -pos: {position}; \n\t prod: {production}";
    }

    public string GetString()
    {
        string connectionsStr = "connections: \n";

        foreach (EnergyNode node in connections)
        {
            connectionsStr += node.GetSimpleString() + "\n";
        }

        return $"--pos: {position}; \n prod: {production}\n {connectionsStr}";
    }

    public bool IsInContact(EnergyNode other)
    {
        var (aMinXMinY, _, _, aMaxXMaxY) = Verts();
        var (bMinXMinY, _, _, bMaxXMaxY) = other.Verts();

        int aMinX = aMinXMinY.x, aMinY = aMinXMinY.y;
        int aMaxX = aMaxXMaxY.x, aMaxY = aMaxXMaxY.y;
        int bMinX = bMinXMinY.x, bMinY = bMinXMinY.y;
        int bMaxX = bMaxXMaxY.x, bMaxY = bMaxXMaxY.y;

        // Vertical edge contact (left/right)
        if (aMaxX == bMinX || bMaxX == aMinX)
        {
            if (Math.Min(aMaxY, bMaxY) > Math.Max(aMinY, bMinY)) return true;
        }

        // Horizontal edge contact (top/bottom)
        if (aMaxY == bMinY || bMaxY == aMinY)
        {
            if (Math.Min(aMaxX, bMaxX) > Math.Max(aMinX, bMinX)) return true;
        }

        return false;
    }

    public (Vector2Int minXminY, Vector2Int minXmaxY, Vector2Int maxXminY, Vector2Int maxXmaxY) Verts()
    {
        Vector2Int minXminY = Vector2Int.zero;
        Vector2Int minXmaxY;
        Vector2Int maxXminY;
        Vector2Int maxXmaxY = Vector2Int.zero;

        bool first = true;

        foreach (var pos in position)
        {
            if (first) {
                first = false;

                minXminY = pos;
                maxXmaxY = pos;

                continue;
            }

            if (pos.x < minXminY.x || pos.y < minXminY.y)
            {
                minXminY = pos;
            } else if (pos.x > maxXmaxY.x || pos.y > maxXmaxY.y)
            {
                maxXmaxY = pos;
            }
        }

        minXmaxY = new(minXminY.x, maxXmaxY.y);
        maxXminY = new(maxXmaxY.x, minXminY.y);

        return (minXminY, minXmaxY, maxXminY, maxXmaxY);
    }

    public override bool Equals(object obj)
    {
        if (obj is not EnergyNode other) return false;
        if (production != other.production) return false;
        
        bool setsEqual = true;

        if (position == null)
        {
            setsEqual = setsEqual && (other.position == null);
        } else
        {
            setsEqual = setsEqual && position.SetEquals(other.position);
        }

        // maybe i don't need to ensure this too?
        if (connections == null)
        {
            setsEqual = setsEqual && (other.connections == null);
        } else
        {
            setsEqual = setsEqual && connections.SetEquals(other.connections);
        }

        return setsEqual;
    }

    public override int GetHashCode()
    {
        // i don't use the EnergyNode set here too because it'll end up 
        // trying to hash the stuff inside the connections hashset and 
        // overflow the stack

        var setComparerPos = HashSet<Vector2Int>.CreateSetComparer();

        return HashCode.Combine(production, setComparerPos.GetHashCode(position));
    }
}

public class EnergyNodeRep
{
    
}

/*
public class EnergyConveyor : EnergyNode
{
    // public HashSet<EnergySourceBundle> connectionsSrc;
    // public HashSet<EnergySink> connectionsDrain;
    public EnergyConveyor(Vector3Int position, HashSet<EnergyNode> connections, float production) : base(position, connections, production)
    {
    }
}

public class EnergySink : EnergyNode
{
    public EnergySink(Vector3Int position, HashSet<EnergyNode> connections, float production) : base(position, connections, production)
    {
    }
}

public class EnergySourceBundle : EnergyNode
{
    public HashSet<EnergySource> ConnectedSources;
}


public class EnergySource : EnergyNode
{
    public EnergySource(Vector3Int position, HashSet<EnergyNode> connections, float production) : base(position, connections, production)
    {
    }
}
*/
    /*
    private void ReactToNewConnections((HashSet<Vector2Int>, HashSet<Vector2Int>) positions, (float?, float?) values)
    {
        // make nodes
        bool node1Exists = nodes.TryGetValue(positions.Item1, out EnergyNode node1);
        bool node2Exists = nodes.TryGetValue(positions.Item2, out EnergyNode node2);
        
        if (node1Exists) node1.production = values.Item1 ?? node1.production;
        
        if (node2Exists) node2.production = values.Item2 ?? node2.production;

        if (node1Exists && node2Exists)
        {
            BridgeNodes(node1, node2);            
        } else if (node1Exists && !node2Exists)
        {
            var connections = new HashSet<EnergyNode>(1)
            {
                node1
            };

            // todo: automate this  maybe? just get it done
            if (values.Item2 == null)
            {
                values.Item2 = getValueOfCell(positions.Item2[0]);
                
                if (values.Item2 == null)
                    Debug.LogError("Energy: production value not provided for nonexistent node, using 0");
            } 

            //create node 2
            node2 = new EnergyNode(positions.Item2, connections, values.Item2.Value);
            NewNode(node2);
        } else if (!node1Exists && node2Exists)
        {
            var connections = new HashSet<EnergyNode>(1)
            {
                node2
            };

            if (values.Item1 == null)
            {
                values.Item1 = getValueOfCell(positions.Item1[0]);
                
                if (values.Item1 == null)
                    Debug.LogError("Energy: production value not provided for nonexistent node, using 0");
            }  

            //create node 2
            node1 = new EnergyNode(positions.Item1, connections, values.Item1.Value);
            NewNode(node1);
        } else
        {
            // both don't exist, create manually both
            if (values.Item1 == null)
            {
                values.Item1 = getValueOfCell(positions.Item1[0]);
                
                if (values.Item1 == null)
                    Debug.LogError("Energy: production value not provided for nonexistent node, using 0");
            }
            
            if (values.Item2 == null)
            {
                values.Item2 = getValueOfCell(positions.Item2[0]);
                
                if (values.Item2 == null)
                    Debug.LogError("Energy: production value not provided for nonexistent node, using 0");
            } 

            node1 = new EnergyNode(positions.Item1, new HashSet<EnergyNode>(), values.Item1.Value);
            NewNode(node1);

            var connections2 = new HashSet<EnergyNode>(1) { node1 };

            node2 = new EnergyNode(positions.Item2, connections2, values.Item2.Value);

            NewNode(node2);
        }

        
    }
    */