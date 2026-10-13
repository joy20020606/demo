# chiller-pilot

多案場冰水主機遙測與控制後端(demo)。

> 狀態:開發中。目前完成階段 1(模擬器、gateway 上行)。

- 規格:[docs/spec.md](docs/spec.md)
- 實作計畫:[docs/plan.md](docs/plan.md)
- 決策紀錄:[docs/decisions/](docs/decisions/)

## 目前可以跑的東西

```
simulator-a ←Modbus→ gateway-a ─┐
                                ├─MQTT→ mosquitto
simulator-b ←Modbus→ gateway-b ─┘
```

兩個 gateway 使用同一個 image,差別只在掛載的點位表(`sites/site_a.yaml`、`sites/site_b.yaml`)。

```powershell
docker compose up -d
docker compose -f docker-compose.site.yml up -d --build
docker compose exec mosquitto mosquitto_sub -t "site/#" -v
```

`docker-compose.yml` 是雲端,`docker-compose.site.yml` 是案場端。兩者不共用網路,案場端只透過 MQTT 往外連。

## 模擬器

兩個以 Modbus TCP 提供資料的模擬冰水主機。兩個案場的暫存器位址、倍率、資料寬度與設備係數都不同。

| | 案場 A | 案場 B |
|---|---|---|
| 溫度 | 位址 0 起,倍率 0.1 | 位址 100 起,倍率 0.01 |
| 耗電 | 位址 9 起,倍率 0.1,一格 | 位址 202 起,單位 W,兩格合成一個數 |
| 設定值 | 位址 20、21 | 位址 300、301 |

### 用 Python 直接執行

```powershell
cd simulator
python -m venv .venv
.\.venv\Scripts\python -m pip install -e ".[dev]"
.\.venv\Scripts\python -m pytest -q
.\.venv\Scripts\python -m sim
```

另開一個終端機讀取:

```powershell
cd simulator
.\.venv\Scripts\python scripts\peek.py
.\.venv\Scripts\python scripts\peek.py --set ct_fan_speed 60
.\.venv\Scripts\python scripts\peek.py --fault scale_error on
```

### 用 Docker 執行兩個案場

```powershell
docker compose -f docker-compose.site.yml up -d --build
cd simulator
.\.venv\Scripts\python scripts\peek.py --port 5020 --profile profiles\site_a.yaml
.\.venv\Scripts\python scripts\peek.py --port 5021 --profile profiles\site_b.yaml
```

## 模擬器的簡化模型

模擬器的用途是讓設定值與耗電之間有合理的關聯,不是精確的設備模型。

- 冷氣需求隨時間與外氣溫度變化。模擬時間預設為實際時間的 60 倍,24 分鐘走完一天。
- 冰水出水溫設定值調高:主機較省,但泵要送更多水,較耗。
- 冷卻水塔風扇轉慢:風扇耗電依轉速三次方下降,但冷卻水變熱,主機較耗。
- 因此總耗電有一個最低點,而且會隨負載移動;兩個案場的最低點不同。

故障開關(以 Modbus coil 控制):`sensor_stuck` 讓出水溫讀值凍結,`scale_error` 讓出水溫讀值放大 10 倍。
