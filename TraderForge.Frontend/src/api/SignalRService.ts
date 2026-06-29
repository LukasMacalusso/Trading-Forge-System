import * as signalR from '@microsoft/signalr';
import { useMarketStore } from '@store/marketStore';
import { useNotificationStore } from '@store/notificationStore';
import type { PendingOperation } from '@models/BotFlow';

export class SignalRService {
  private static connection: signalR.HubConnection | null = null;

  public static async startConnection() {
    if (this.connection) return;

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl('/hubs/marketdata')
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReceivePriceUpdate', (prices: Record<string, number>) => {
      const updatePrice = useMarketStore.getState().updateAssetPrice;

      for (const [symbol, newPrice] of Object.entries(prices)) {
        // Backend sends us the live price via SignalR
        updatePrice(symbol, newPrice, 0);
      }
    });

    // Bot actions awaiting manual authorization (FR-14). No-op until the
    // backend (4.3 — Notification Bots & Manual Approvals) emits these events.
    this.connection.on('ReceivePendingOperation', (operation: PendingOperation) => {
      const { pendingOperations, setPendingOperations, addNotification } = useNotificationStore.getState();
      if (pendingOperations.some((op) => op.id === operation.id)) return;
      setPendingOperations([...pendingOperations, operation]);
      addNotification('warning', `Un bot requiere tu autorización: ${operation.action === 'Buy' ? 'comprar' : 'vender'} ${operation.symbol}.`);
    });

    this.connection.on('RemovePendingOperation', (operationId: string) => {
      useNotificationStore.getState().removePendingOperation(operationId);
    });

    try {
      await this.connection.start();
      console.log('📶 SignalR Live Market Data Connected.');
    } catch (err) {
      console.error('📶 SignalR Connection Error: ', err);
    }
  }

  public static stopConnection() {
    if (this.connection) {
      this.connection.stop();
      this.connection = null;
    }
  }
}
