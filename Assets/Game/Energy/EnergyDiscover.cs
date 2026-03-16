using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class DiscoverEnergySystem : MonoBehaviour
{
    Dictionary<Vector3Int, EnergyNode> nodes;

    List<Network> networks;

    #nullable enable
    private Network? GetNetworkOfNode(EnergyNode node)
    {
        foreach (Network net in networks)
        {
            if (net.networkNodes.Values.Contains(node)) return net;
        }

        return null;
    }

    public void NewNode(EnergyNode node)
    {
        nodes.Add(node.position, node);

        #nullable enable
        Network? connectedNetwork = null;

        int needsBridging = 0;

        foreach (Network network in networks)
        {
            bool bridgeCheck = false;
            foreach (EnergyNode connected in network.networkNodes.Values.Where(x => node.connections.Contains(x)))
            {
                if (!bridgeCheck)
                {
                    needsBridging += 1;
                    bridgeCheck = true;
                }

                connectedNetwork = network;
                // this is kinda sketchy but eh
                connected.connections.Add(node);
            }
        }

        if (connectedNetwork != null)
        {
            // existing network was found
            connectedNetwork.AddNode(node);

            if (needsBridging != 1)
            {
                // TODO: handle network bridging
                
            }
        } else
        {
            // no existing network was found, make new
            Network network = new Network();
            network.AddNode(node);       
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
        _networkNodes = new Dictionary<Vector3Int, EnergyNode>();
        networkNodes = new(_networkNodes);
        netValue = 0f;
    }

    private Dictionary<Vector3Int, EnergyNode> _networkNodes;
    public ReadOnlyDictionary<Vector3Int, EnergyNode> networkNodes;

    public float netValue;

    public void AddNode(EnergyNode node)
    {
        netValue += node.production;
        _networkNodes.Add(node.position, node);
    }

    public void MergeNetwork(Network network, EnergyNode node1, EnergyNode node2)
    {
        _networkNodes.AddRange(network._networkNodes);
        _networkNodes[node1.position].connections.Add(node2);
        _networkNodes[node2.position].connections.Add(node1);
    }
}

public abstract class EnergyNode
{
    public EnergyNode(Vector3Int position, HashSet<EnergyNode> connections, float production)
    {
        this.connections = connections;
        this.position = position;
        this.production = production;
    }
    public HashSet<EnergyNode> connections;
    public Vector3Int position;
    public float production; // can be negative
}

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

/*
public class EnergySourceBundle : EnergyNode
{
    public HashSet<EnergySource> ConnectedSources;
}
*/

public class EnergySource : EnergyNode
{
    public EnergySource(Vector3Int position, HashSet<EnergyNode> connections, float production) : base(position, connections, production)
    {
    }
}