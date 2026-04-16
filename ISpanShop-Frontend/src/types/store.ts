export interface SellerKpi {
  totalRevenue: number;
  totalOrders: number;
  pendingOrders: number;
  totalProducts: number;
  lowStockCount: number;
}

export interface ApexChartSeries {
  name: string;
  data: number[];
}

export interface ApexChartData {
  labels: string[];
  series: ApexChartSeries[];
}

export interface TopProduct {
  productName: string;
  salesVolume: number;
  salesRevenue: number;
}

export interface SellerDashboardData {
  kpis: SellerKpi;
  salesTrend: ApexChartData;
  topProducts: TopProduct[];
}
