using System;
using NumSharp;


public class SaveData
{
    public double[][][][] QTable { get; set; } 

    public SaveData()
    {
        this.QTable = np.zeros(20, 20, 20, 2);
    }

}
