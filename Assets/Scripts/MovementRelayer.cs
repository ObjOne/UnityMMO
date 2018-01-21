﻿using MMOServer;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Attach this script to an actor object which you expect to be able to move.
/// </summary>
public class MovementRelayer : MonoBehaviour {

    private bool actorMoving;
    private PlayerMovement mover;
    private Connection connection;
    public float networkTick = 0.01f;

    void Start()
    {
        connection = GameObject.Find("WorldServerConnection").GetComponent<Connection>();
        mover = gameObject.GetComponent<PlayerMovement>();
        InvokeRepeating("Flush", 0.0f, networkTick);

    }

    void Update()
    {
        if (mover.IsMoving)
        {
            var packets = connection.GetQueue();
            PositionPacket posPacket = new PositionPacket(gameObject.transform.position.x, gameObject.transform.position.y, true, Data.CHARACTER_ID);
            if (packets.Any())
            {
                PositionPacket lastPacket = new PositionPacket(packets.Last().data);
                if (posPacket.XPos != lastPacket.XPos || posPacket.YPos != lastPacket.YPos)
                {
                    SubPacket sp = new SubPacket(GamePacketOpCode.PositionPacket, Data.CHARACTER_ID, 0, posPacket.GetBytes(), SubPacketTypes.GamePacket);
                    connection.QueuePacket(sp);
                }
            }
            else
            {
                SubPacket sp = new SubPacket(GamePacketOpCode.PositionPacket, Data.CHARACTER_ID, 0, posPacket.GetBytes(), SubPacketTypes.GamePacket);
                connection.QueuePacket(sp);
            }
        }
    }


    //this whole queue stuff might be unnecessary, it's only the latest position set that really matters
    public void Flush()
    {
        connection.FlushQueuedSendPackets();
    }

}
