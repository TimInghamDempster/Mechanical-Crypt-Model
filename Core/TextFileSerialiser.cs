using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Core
{
    public enum SerialiseTypes
    {
        intType,
        floatType,
        stringType,
        listType
    }

    public class TextFileSerialiser
    {
        public static void DeSerialise(Stream data, dynamic objectToFill)
        {
            var reader = new StreamReader(data);

            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                var splitLine = line.Split(':');
                
                if (splitLine.Length != 3)
                {
                    Logger.Log("Wrong number of arguments in data line");
                    return;
                }

                SerialiseTypes dataType;
                var validDataType = Enum.TryParse(splitLine[2], out dataType);

                if (!validDataType)
                {
                    Logger.Log("Invalid data type: " + splitLine[2]);
                    return;
                }

                if (dataType != SerialiseTypes.listType)
                {
                    //serialiseData.AddItem(splitLine[0], splitLine[1], dataType);
                }
                else
                {
                }
            }
        }
        public static void Serialise(Stream writer, object data)
        {
        }
    }
}
