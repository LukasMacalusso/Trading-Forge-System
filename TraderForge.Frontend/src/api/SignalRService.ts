import * as signalR from '@microsoft/signalr';
import { useMarketStore } from '@store/marketStore';

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
