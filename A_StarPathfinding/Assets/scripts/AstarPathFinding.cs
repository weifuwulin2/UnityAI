using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class PathMarker
{
    public MapLocation location;
    public float G;
    public float H;
    public float F;
    public GameObject marker;
    public PathMarker parent;

    public PathMarker(MapLocation l, float g, float h, float f, GameObject marker, PathMarker p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            return false;
        else
            return location.Equals(((PathMarker)obj).location);
    }

    public override int GetHashCode()
    {
        return 0;
    }

}

public class AstarPathFinding : MonoBehaviour
{
 
    public Maze maze;
    public Material closedMaterial;
    public Material openMaterial;

    //two lists for closed and opened
    List<PathMarker> open = new List<PathMarker>();
    List<PathMarker> closed = new List<PathMarker>();

    public GameObject start;
    public GameObject end;
    public GameObject pathP;

    PathMarker goalNode;
    PathMarker startNode;

    PathMarker lastPos;
    // be true when we find the correct path
    bool done = false;

    void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject marker in markers)
        {
            Destroy(marker);
        }
    }

    //generate random start and end point
    void BeginSearch()
    {
        done = false;
        RemoveAllMarkers();

        //pick two position in the maze for start and end
        List<MapLocation> locations = new List<MapLocation>();
        for (int z = 0; z < maze.depth - 1; z++)
        {
            for (int x = 0; x < maze.width; x++)
            {
                print(maze.map[x, z]);
                if (maze.map[x,z]!=1)
                {
                    locations.Add(new MapLocation(x, z));
                }
            }
        }

        locations.Shuffle();

        Vector3 startLocation = new Vector3(locations[0].x * maze.scale,0,locations[0].z * maze.scale);
        startNode = new PathMarker(new MapLocation(locations[0].x, locations[0].z), 0, 0, 0,
            Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 goalLocation = new Vector3(locations[1].x * maze.scale, 0, locations[1].z * maze.scale);
        goalNode = new PathMarker(new MapLocation(locations[1].x, locations[1].z), 0, 0, 0,
            Instantiate(end, goalLocation, Quaternion.identity), null);

        //clear open and closed list
        open.Clear();
        closed.Clear();
        open.Add(startNode);
        lastPos = startNode;
    }

    void Search(PathMarker thisNode)
    {
        if (thisNode == null)
        {
            return;
        }
        //goal has been found
        if (thisNode.Equals(goalNode))
        {
            done = true;
            return;
        }
        //loop through and find all of the neighbors of current position
        foreach (MapLocation dir in maze.directions)
        {
            MapLocation neighbour = dir + thisNode.location;
            //1. it shouldn't be a wall, if it's a wall then go to next neighbor
            if (maze.map[neighbour.x,neighbour.z] == 1)
            {
                continue;
            }
            //2. if the neighbour if out of the maze, if is, ocntinue
            if (neighbour.x<1 || neighbour.x>=maze.width || neighbour.z<1 || neighbour.z>=maze.depth)
            {
                continue;
            }
            //3. if the neighbour is in the closed list, continue
            if (IsClosed(neighbour))
            {
                continue;
            }
            //4. calculate G H F (cost involved in travelling)
            //distance from the current node to the neighbour, plus the distance that has been travelled the entire way up to this point
            float G = Vector2.Distance(thisNode.location.ToVector(),neighbour.ToVector()) + thisNode.G;
            //distance between this and the final goal
            float H = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
            //summary of our path distance or the cost involved in travelling
            float F = G + H; 

            //create a gameobject to put on the path to the particular spot
            GameObject pathBlock = Instantiate(pathP, new Vector3(neighbour.x * maze.scale, 0, neighbour.z * maze.scale), Quaternion.identity);

            //get the textmesh attached to the object, and show it 
            TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
            values[0].text = "G: " + G.ToString("0.00");
            values[1].text = "H: " + H.ToString("0.00");
            values[2].text = "F: " + F.ToString("0.00");

            //5. add the marker on the open list
            //make sure the marker is not already exist in the list
            if (!UpdateMarker(neighbour,G,H,F,thisNode))
            {
                open.Add(new PathMarker(neighbour, G, H, F, pathBlock, thisNode));
            }
        }

        //6. pick one of the marker on the open list that's going to become the next node that we're focusing on so the position that's going to generate the next set of neighbours
        //chose the one with the lowest F value, easiest way: reorder your open list from the smallest to the largest F value, then order by the smallest H value
        open = open.OrderBy(p => p.F).ThenBy(n=>n.H).ToList();
        PathMarker pm = (PathMarker)open.ElementAt(0);
        //close the pathmarker
        closed.Add(pm);
        open.RemoveAt(0);
        pm.marker.GetComponent<Renderer>().material = closedMaterial;

        lastPos= pm; 
    }

    //7. getting the path when it hits goal, basically find the parent of the last goal node, and then find the parent of that node, and so on until we get to the start node
    void GetPath()
    {
        RemoveAllMarkers();
        PathMarker begin = lastPos;
        while (begin != null && !begin.Equals(startNode))
        {
            Instantiate(pathP, new Vector3(begin.location.x * maze.scale, 0, begin.location.z * maze.scale), Quaternion.identity);
            begin = begin.parent;
        }

        //put the starting node 
        Instantiate(pathP,new Vector3(startNode.location.x * maze.scale,0, startNode.location.z * maze.scale),Quaternion.identity);
    }

    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach (PathMarker p in open)
        {
            if (p.location.Equals(pos))
            {
                if (p.F > f)
                {
                    p.F = f;
                    p.G = g;
                    p.H = h;
                    p.parent = prt;
                    return true;
                }
                return false;
            }
        }
        return false;
    }

    bool IsClosed(MapLocation marker)
    {
        foreach (PathMarker p in closed)
        {
            if (p.location.Equals(marker))
            {
                return true;
            }
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            BeginSearch();
        }
        if (Input.GetKeyDown(KeyCode.C) && !done)
        {
            Search(lastPos);
        }
        if(Input.GetKeyDown(KeyCode.M))
        {
            GetPath();
        }
    }
}
