﻿using ShortDev.Microsoft.ConnectedDevices.Messages;
using ShortDev.Microsoft.ConnectedDevices.Messages.Session;
using ShortDev.Microsoft.ConnectedDevices.Platforms;
using System;
using System.Collections.Generic;

namespace ShortDev.Microsoft.ConnectedDevices;

/// <summary>
/// Provides the interface between a <see cref="CdpAppBase"/> and a <see cref="CdpSession"/>.
/// </summary>
public sealed class CdpChannel : IDisposable
{
    internal CdpChannel(CdpSession session, ulong channelId, CdpAppBase handler, CdpSocket socket)
    {
        Session = session;
        ChannelId = channelId;
        MessageHandler = handler;
        Socket = socket;
    }

    /// <summary>
    /// Get's the corresponding <see cref="CdpSession"/>. <br/>
    /// <br/>
    /// <inheritdoc cref="CdpSession"/>
    /// </summary>
    public CdpSession Session { get; }

    /// <summary>
    /// Get's the corresponding <see cref="CdpSocket"/>. <br/>
    /// <br/>
    /// <inheritdoc cref="CdpSocket" />
    /// </summary>
    public CdpSocket Socket { get; }

    /// <summary>
    /// Get's the corresponding <see cref="CdpAppBase"/>. <br/>
    /// <br/>
    /// <inheritdoc cref="CdpAppBase"/>
    /// </summary>
    public CdpAppBase MessageHandler { get; }

    /// <summary>
    /// Get's the unique id for the channel. <br/>
    /// The id is unique as long as the channel is active.
    /// </summary>
    public ulong ChannelId { get; }

    public void HandleMessageAsync(CdpMessage msg)
        => MessageHandler.HandleMessage(msg);

    public void SendBinaryMessage(BodyCallback bodyCallback, uint msgId, List<AdditionalHeader>? headers = null)
    {
        CommonHeader header = new()
        {
            Type = MessageType.Session,
            ChannelId = ChannelId
        };

        if (headers != null)
            header.AdditionalHeaders = headers;

        Session.SendMessage(Socket, header, writer =>
        {
            new BinaryMsgHeader()
            {
                MessageId = msgId
            }.Write(writer);
            bodyCallback(writer);
        });
    }

    void IDisposable.Dispose()
        => Dispose();

    public void Dispose(bool closeSession = false, bool closeSocket = false)
    {
        Session.UnregisterChannel(this);
        if (closeSocket)
            Socket.Dispose();
        if (closeSession)
            Session.Dispose();
    }
}
