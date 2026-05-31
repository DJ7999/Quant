import OptionHistoryManager from "./OptionHistoryManager";
import MoneyMarketHistoryManager from "./MoneyMarketHistorymanager";
import OptionBasketVisualizer from "./OptionBasketVisualizer";
import MlModelManager from "./MlModelManager"; // Imported

export const QUANT_APPS = [
  {
    id: "option-history-manager",
    title: "Option History Manager",
    icon: "📂",
    category: "DATA ADMINISTRATION",
    component: OptionHistoryManager,
  },
  {
    id: "money-market-history-manager",
    title: "Money Market History Manager",
    icon: "📂",
    category: "DATA ADMINISTRATION",
    component: MoneyMarketHistoryManager,
  },
  {
    id: "option-basket-visualizer",
    title: "Option Basket Visualizer",
    icon: "📊",
    category: "ANALYTICS",
    component: OptionBasketVisualizer,
  },
  {
    id: "ml-model-manager", // New Entry
    title: "ML Model Manager",
    icon: "🤖",
    category: "ANALYTICS", // Or "DATA ADMINISTRATION" depending on your preference
    component: MlModelManager,
  }
];