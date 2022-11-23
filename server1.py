from flask import Flask, request, jsonify
from AcomodarCajasModel1 import *

# Size of the board:
Number_agents = 5
width = 28
height = 28
AcomodarCajasModel = None
currentStep = 0

app = Flask("Actividad Integradora Acomodar Cajas")

# @app.route('/', methods=['POST', 'GET'])


@app.route('/init', methods=['POST', 'GET'])
def initModel():
    global currentStep, AcomodarCajasModel, Number_agents, width, height

    if request.method == 'POST':
        Number_agents = int(request.form.get('NAgents'))
        width = int(request.form.get('width'))
        height = int(request.form.get('height'))
        currentStep = 0
        AcomodarCajasModel = AcomodarCajasModel(Number_agents, width, height)

        return jsonify({"message": "Parameters recieved, model initiated."})


@app.route('/getWalle', methods=['GET'])
def getWalle():
    global AcomodarCajasModel

    if request.method == 'GET':
        wallePositions = [{"id": str(a.unique_id), "x": x, "y": 1, "z": z, "box": a.box} 
                        for (a, x, z) in AcomodarCajasModel.grid.coord_iter() 
                        for a in a if isinstance(a, Walle)]

        return jsonify({'positions': wallePositions})


@app.route('/getCajas', methods=['GET'])
def getCajas():
    global AcomodarCajasModel

    if request.method == 'GET':
        cajaPosition = [{"id": str(a.unique_id), "x": x, "y": 1, "z": z,
                       "recoge": a.recoge} for (a, x, z) in
                       randomModel.grid.coord_iter() 
                       for (a, x, z) in AcomodarCajasModel.grid.coord_iter() 
                       for a in a if isinstance(a, Cajas)]

        return jsonify({'positions': cajaPosition})


@app.route('/getRepisas', methods=['GET'])
def getPilas():
    global AcomodarCajasModel

    if request.method == 'GET':
        repisaPos = []
        contador = 0
        for x, y in AcomodarCajasModel.repisa.keys():
            repisaPos.append({"id": count, "x": x, "y": 0, "z": y, "numero": AcomodarCajasModel.repisa[
                                            (x, y)]})
            count += 1
        return jsonify({'positions': repisaPos})


@app.route('/update', methods=['GET'])
def updateModel():
    global currentStep, randomModel
    if request.method == 'GET':
        AcomodarCajasModel.step()
        currentStep += 1
        return jsonify({'message': f'Model updated to step {currentStep}.',
                       'currentStep': currentStep})


if __name__ == '__main__':
    app.run(host="localhost", port=8521, debug=True)
