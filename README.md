## MarketMaker

The MarketMaker is a extensible open source market maker bot for okex dex

### 1.Components

#### a. Account management component

The component is responsible for creating account and getting account info, etc

#### b. Fair value component

The component is responsible for getting latest information and calculating fair value / market state

#### c. Quote component

The component is responsible for managing buy and sell pending orders on market based on fair value and quote strategy. If the market state is abnormal, it should cancel all pending orders

#### d. Risk control component

The component is responsible for risk control, include but not limited to hedge exposure, pnl monitoring, etc

### 2.Installation

```
git clone https://github.com/Kevin-OpenSource-Labs/MarketMaker.git
```
