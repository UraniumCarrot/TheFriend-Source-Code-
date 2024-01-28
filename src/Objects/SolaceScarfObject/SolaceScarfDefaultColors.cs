using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace TheFriend.Objects.SolaceScarfObject;

public class SolaceScarfDefaultColors
{
    public static List<Color> SolaceScarfDefaultColor(SolaceScarfAbstract scarf)
    {
        List<Color> list;
        switch (scarf.regionOrigin)
        {
            case "SU": list = [new Color(0.50f,0.6f,1.0f),new Color(0.8f,1.0f,1.0f)]; break; // Outskirts
            case "HI": list = [new Color(0.00f,0.2f,1.0f),new Color(0.0f,0.8f,1.0f)]; break; // Industrial Complex
            case "DS": list = [new Color(0.0f,0.25f,0.3f),new Color(0f,0.12f,0.15f)]; break; // Drainage System
            case "LF": list = [new Color(0.60f,0.1f,0.1f),new Color(1.0f,1.0f,0.5f)]; break; // Farm Arrays
            case "SL": list = [new Color(0.0f,0.25f,0.3f),new Color(0.7f,1.0f,0.0f)]; break; // Shoreline
            case "SI": list = [new Color(0.40f,0.7f,1.0f),new Color(0.7f,1.0f,1.0f)]; break; // Sky Islands
            case "SH": list = [new Color(0.10f,0.1f,0.2f),new Color(0.9f,0.9f,1.0f)]; break; // Shaded Citadel
            case "SB": list = [new Color(0.21f,0.2f,0.2f),new Color(1.0f,1.0f,0.7f)]; break; // Subterranean
            case "UW": list = [new Color(0.20f,0.0f,0.0f),new Color(0.0f,0.2f,0.0f)]; break; // The Exterior
            case "CC": list = [new Color(1.00f,1.0f,0.5f),new Color(1.0f,0.8f,0.0f)]; break; // Chimney Canopy
            case "SS": list = [new Color(1.00f,0.5f,0.0f),new Color(1.0f,0.2f,0.0f)]; break; // Five Pebbles
            case "GW": list = [new Color(0.00f,0.2f,0.0f),new Color(0.0f,0.0f,0.0f)]; break; // Garbage Wastes
            
            case "OE": list = [new Color(1.00f,0.7f,0.7f),new Color(1.0f,1.0f,0.6f)]; break; // Outer Expanse
            case "DM": list = [new Color(1.00f,0.5f,0.0f),new Color(1.0f,0.2f,0.0f)]; break; // Looks To The Moon (spearmaster moon)
            case "CL": list = [new Color(0.00f,0.0f,0.0f),new Color(0.3f,0.2f,0.0f)]; break; // Silent Construct
            case "VS": list = [new Color(0.31f,0.3f,0.3f),new Color(0.6f,0.5f,0.3f)]; break; // Pipeyard
            case "LC": list = [new Color(0.85f,0.8f,0.6f),new Color(0.7f,0.0f,0.0f)]; break; // Metropolis
            case "RM": list = [new Color(0.00f,0.0f,0.0f),new Color(0.0f,0.0f,1.0f)]; break; // The Rot
            case "UG": list = [new Color(0.20f,0.5f,0.0f),new Color(0f,0.12f,0.15f)]; break; // Undergrowth
            case "LM": list = [new Color(0.20f,0.0f,0.0f),new Color(0.7f,1.0f,0.0f)]; break; // Waterfront Facility
            case "MS": list = [new Color(1.00f,0.5f,0.0f),new Color(1.0f,0.2f,0.0f)]; break; // Submerged Superstructure
            case "HR": list = [new Color(1.00f,0.0f,0.0f),new Color(1.0f,1.0f,0.0f)]; break; // Rubicon

            case "NotARegion" or "": list = [Color.white,Color.white]; break;
            default:
                var state = Random.state;
                Random.InitState(scarf.regionOrigin.First()+scarf.regionOrigin.Last());
                list = [Extensions.RandomRGB(), Extensions.RandomRGB()];
                scarf.ID = new EntityID(-1, scarf.regionOrigin.First()+scarf.regionOrigin.Last());
                Random.state = state;
                break;
        }
        return list;
    }
}