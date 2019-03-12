using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour
{
    [SerializeField]
    int sizeX, sizeZ;
    [SerializeField]
    HexCell cellPrefab;
    [SerializeField]
    Text cellLabelPrefab;

    HexCell[] cells;
    Canvas gridCanvas;
    HexMesh hexMesh;

    public Color defaultColor = Color.white;


    void Awake(){
        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[sizeX * sizeZ];

        for(int z = 0, i = 0; z < sizeZ; z++){
            for(int x = 0; x < sizeX; x++){
                CreateCell(x,z,i++);
            }
        }
    }

    void Start(){
        hexMesh.TriangulateAll(cells);
    }

  

    




    /* Creates and labels a Hex Cell */
    void CreateCell(int x, int z, int i){
        Vector3 pos;
        pos.x = (x + (z * 0.5f - (z / 2))) * (HexMetrics.innerRadius * 2f);
        pos.y = 0;
        pos.z = z * (HexMetrics.outerRadius * 1.5f);

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform,false);
        cell.transform.localPosition = pos;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x,z);
        cell.color = defaultColor;

        //label cell
        Text label = Instantiate<Text>(cellLabelPrefab);
		label.rectTransform.SetParent(gridCanvas.transform, false);
		label.rectTransform.anchoredPosition = new Vector2(pos.x, pos.z);
		label.text = cell.coordinates.ToString();
    }

    /* colors a cell at a given position */
    public void ColorCell(Vector3 position, Color color) {
		position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        int index = coordinates.X + coordinates.Z * sizeX + coordinates.Z / 2;
        HexCell cell = cells[index];
        cell.color = color;
        hexMesh.TriangulateAll(cells);
    }

}
