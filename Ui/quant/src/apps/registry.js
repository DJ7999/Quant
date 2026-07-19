import OptionHistoryManager from "./OptionHistoryManager";
import MoneyMarketHistoryManager from "./MoneyMarketHistorymanager";
import OptionBasketVisualizer from "./OptionBasketVisualizer";
import MlModelManager from "./MlModelManager";
import HypothesisTesting from "./HypothesisTesting";

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
    id: "ml-model-manager",
    title: "ML Model Manager",
    icon: "🤖",
    category: "ANALYTICS",
    component: MlModelManager,
  },
  {
    id: "hypothesis-testing",
    title: "Hypothesis Testing",
    icon: "🧪",
    category: "ANALYTICS",
    component: HypothesisTesting,
  }
];