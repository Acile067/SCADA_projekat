using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus write single register functions/requests.
    /// </summary>
    public class WriteSingleRegisterFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WriteSingleRegisterFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public WriteSingleRegisterFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusWriteCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            ModbusWriteCommandParameters parameters = CommandParameters as ModbusWriteCommandParameters;

            byte[] request = new byte[12];

            request[0] = BitConverter.GetBytes(parameters.TransactionId)[1];
            request[1] = BitConverter.GetBytes(parameters.TransactionId)[0];
            request[2] = BitConverter.GetBytes(parameters.ProtocolId)[1];
            request[3] = BitConverter.GetBytes(parameters.ProtocolId)[0];
            request[4] = BitConverter.GetBytes(parameters.Length)[1];
            request[5] = BitConverter.GetBytes(parameters.Length)[0];
            request[6] = parameters.UnitId;
            request[7] = parameters.FunctionCode;
            request[8] = BitConverter.GetBytes(parameters.OutputAddress)[1];
            request[9] = BitConverter.GetBytes(parameters.OutputAddress)[0];
            request[10] = BitConverter.GetBytes(parameters.Value)[1];
            request[11] = BitConverter.GetBytes(parameters.Value)[0];

            return request;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> responseDictionary = new Dictionary<Tuple<PointType, ushort>, ushort>();

            ushort outputAddress = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(response, 8));
            ushort value = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(response, 10));

            responseDictionary.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, outputAddress), value);

            return responseDictionary;
        }
    }
}