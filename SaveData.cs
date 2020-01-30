using System;
using NumSharp;


public class SaveData
{
    public NDarray QTable { get; set; } 

    public SaveData()
    {
        this.QTable = np.zeros(20, 20, 20, 2);
    }

}
