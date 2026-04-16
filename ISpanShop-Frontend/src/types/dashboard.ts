export interface DashboardKpi {
  netRevenue: number;
  prevNetRevenue: number;
  totalOrders: number;
  prevTotalOrders: number;
  returnOrders: number;
  prevReturnOrders: number;
  totalItemsSold: number;
  prevTotalItemsSold: number;
  pendingShipmentCount: number;
  pendingRefundCount: number;
  lowStockProductCount: number;
}

export interface ChartSeries {
  name: string;
  data: number[];
}

export interface ApexChartData {
  labels: string[];
  series: ChartSeries[];
}

export interface TopProductSales {
  productName: string;
  categoryName: string;
  salesVolume: number;
  salesRevenue: number;
}
