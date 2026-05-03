import OptionHistoryManager from "./OptionHistoryManager";
import MoneyMarketHistoryManager from "./MoneyMarketHistorymanager"


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
  }
];