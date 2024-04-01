using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            byte[] paket = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.TransactionId)), 0, paket, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.ProtocolId)), 0, paket, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.Length)), 0, paket, 4, 2);
            paket[6] = CommandParameters.UnitId;
            paket[7] = CommandParameters.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)((ModbusReadCommandParameters)CommandParameters).StartAddress)), 0, paket, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)((ModbusReadCommandParameters)CommandParameters).Quantity)), 0, paket, 10, 2);

            return paket;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            var ret = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == CommandParameters.FunctionCode + 0x80)   //provera da li je doslo do greske
            {
                HandeException(response[8]);
            }
            else
            {
                ushort adresa = ((ModbusReadCommandParameters)CommandParameters).StartAddress;  //startna adresa
                ushort value;   //ovde ce uvek biti vrednost
                for (int i = 0; i < response[8]; i = i + 2) //response[8] -> byteCount tj. koliko imamo bajtova u delu koji nosi vrednosti signala
                { //i se uvecava za dva jer preuzimamo short vrednosti
                    value = BitConverter.ToUInt16(response, (i + 9));   //prva vrednost
                    value = (ushort)IPAddress.NetworkToHostOrder((short)value); //prebacivanje u host order
                    ret.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, adresa), value); //cuvanje
                    adresa++;
                }
            }

            return ret;
        }
    }
}