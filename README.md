## Power Production Plan API

This API endpoint, `PostProductionPlan`, performs calculations to determine the optimal power production allocation based on fuel costs, power plant efficiency, and load requirements. The response provides the power plant names and their respective power production values.

### Step 1: Extract data from the payload
The method takes in a payload of type `PowerPlantData` in the request body and extracts its properties: `load`, `fuels`, and `powerplants`.

### Step 2: Calculate the merit-order based on fuel costs taking into account efficiency
The method calculates the cost for each power plant based on its fuel type and efficiency using a lambda expression and the `OrderBy` method. If the power plant is of type "gasfired," the cost is calculated by multiplying the gas fuel cost with the inverse of the efficiency. If the power plant is of type "turbojet," the cost is calculated by multiplying the kerosine fuel cost with the inverse of the efficiency. The power plants are sorted in ascending order of cost multiplied by the minimum power (`pmin`).

### Step 3: Allocate power production for each power plant
The method iterates through each power plant in the sorted order. If the load is not zero, power production is allocated to the power plant based on its type. For "windturbine" power plants, the power production is calculated based on the maximum power (`pmax`) and the wind fuel availability (`fuels.wind`). For "gasfired" or "turbojet" power plants, the power output is determined by considering the load and ensuring it is within the minimum (`pmin`) and maximum (`pmax`) power limits. The load is updated accordingly after each power plant allocation. If the load is already distributed or becomes zero, a power production value of 0 is added to the list.

### Adjust the last power plant's production to match the pmax load and cancel the the  expensive load which will be always tj1 turbojet using kerosine
In the provided lines of code, there is a check to determine if the last power plant in the powerProductions list (denoted by lastIndexHasP - 1) has reached its maximum power production limit (pmax value). Here is an explanation of the code:

    lastIndexHasP refers to the index of the last power plant in the powerProductions list.
    sortedPowerPlants.ElementAt(lastIndexHasP - 1).pmax retrieves the pmax value of the last power plant from the sortedPowerPlants list.
    The condition powerProductions[lastIndexHasP - 1] < sortedPowerPlants.ElementAt(lastIndexHasP - 1).pmax checks if the last power plant's current production is less than its maximum limit.

If the condition is true, it means that the last power plant can still produce more power. In this case, the code block inside the if statement is executed:

    powerProductions[lastIndexHasP - 1] += powerProductions[0]; adds the power production value of the first power plant (powerProductions[0]) to the last power plant's production (powerProductions[lastIndexHasP - 1]). This effectively assigns the first power plant's load to the last power plant.
    powerProductions[0] = 0; sets the power production value of the first power plant to zero since its load has been transferred to the last power plant.

This code block is likely included to redistribute the power load and ensure that the last power plant utilizes its full capacity if there is still remaining power to be allocated.

### Round the power production values to the nearest multiple of 0.1 MW
All power production values are rounded to the nearest multiple of 0.1 MW using the `Math.Round` method.

### Step 4: Create the response JSON object
The response object is created with the power plant names and their corresponding power production values. The power plants are based on the sorted order from Step 2. For each power plant, the name and power production value from the `powerProductions` list are selected.

### Return the response
The response object is returned as an HTTP 200 OK response using the `Ok` method.


### Example Payload

```json
{
  "load": 480,
  "fuels": {
    "gas": 13.4,
    "kerosine": 50.8,
    "co2": 20,
    "wind": 60
  },
  "powerplants": [
    {
      "name": "gasfiredbig1",
      "type": "gasfired",
      "efficiency": 0.53,
      "pmin": 100,
      "pmax": 460
    },
    {
      "name": "gasfiredbig2",
      "type": "gasfired",
      "efficiency": 0.53,
      "pmin": 100,
      "pmax": 460
    },
    {
      "name": "gasfiredsomewhatsmaller",
      "type": "gasfired",
      "efficiency": 0.37,
      "pmin": 40,
      "pmax": 210
    },
    {
      "name": "tj1",
      "type": "turbojet",
      "efficiency": 0.3,
      "pmin": 0,
      "pmax": 16
    },
    {
      "name": "windpark1",
      "type": "windturbine",
      "efficiency": 1,
      "pmin": 0,
      "pmax": 150
    },
    {
      "name": "windpark2",
      "type": "windturbine",
      "efficiency": 1,
      "pmin": 0,
      "pmax": 36
    }
  ]
}


### Example Response for the above payload1.json

```json
{
  "powerplants": [
    {
      "name": "tj1",
      "p": 0
    },
    {
      "name": "windpark1",
      "p": 90
    },
    {
      "name": "windpark2",
      "p": 21.6
    },
    {
      "name": "gasfiredsomewhatsmaller",
      "p": 210
    },
    {
      "name": "gasfiredbig1",
      "p": 159
    },
    {
      "name": "gasfiredbig2",
      "p": 0
    }
  ]
}



### Example Response for the above payload2.json

```json
{
  "powerplants": [
    {
      "name": "tj1",
      "p": 0
    },
    {
      "name": "windpark1",
      "p": 0
    },
    {
      "name": "windpark2",
      "p": 0
    },
    {
      "name": "gasfiredsomewhatsmaller",
      "p": 210
    },
    {
      "name": "gasfiredbig1",
      "p": 270
    },
    {
      "name": "gasfiredbig2",
      "p": 0
    }
  ]
}

### Example Response for the above payload3.json

```json

{
  "powerplants": [
    {
      "name": "tj1",
      "p": 0
    },
    {
      "name": "windpark1",
      "p": 90
    },
    {
      "name": "windpark2",
      "p": 21.6
    },
    {
      "name": "gasfiredsomewhatsmaller",
      "p": 210
    },
    {
      "name": "gasfiredbig1",
      "p": 460
    },
    {
      "name": "gasfiredbig2",
      "p": 129
    }
  ]
}
