import mesa


def movimientos_agentes(model):
    movimientoAgentes = [agent.movimientos for agent in model.schedule.agents]
    return sum(movimientoAgentes)


def calc_tiempo(model):
    """
    It returns the current time of the model

    :param model: the model object
    :return: The time of the model.
    """
    return model.schedule.time


class Walle(mesa.Agent):
    def __init__(self, unique_id: str, model: mesa.Model) -> None:
        super().__init__(unique_id, model)
        self.movimientos = 0
        self.carga = False
        self.cajaCargada = None
        self.type = "Robot"

    def revisarRepisa(self, cell) -> None:
        cellmates = self.model.grid.get_cell_list_contents([self.pos])
        for repisa in cellmates:
            if(repisa.type == "Repisa"):
                if(repisa.tiddy < 5):
                    self.dejar()
                    repisa.tiddy += 1
                elif(repisa.tiddy == 5):
                    repisa.empty = False
                    print("Estoy llena: " + repisa.unique_id + " " +
                          str(repisa.tiddy))

    def dejar(self) -> None:
        self.carga = False
        self.cajaCargada = None
        self.model.numCajas -= 1

    def cargar(self, cell) -> None:
        self.carga = True
        caja = self.model.grid.get_cell_list_contents([cell])[0]
        self.cajaCargada = caja
        caja.cargada = True
        self.model.cajasLista.remove(cell)

    def moverCargado(self):
        posibleMovimiento = self.model.grid.get_neighborhood(
            self.pos,
            moore=False,
            include_center=False)
        nuevaPosicion = self.random.choice(posibleMovimiento)
        self.model.grid.move_agent(self, (nuevaPosicion))
        self.model.grid.move_agent(self.cajaCargada, (nuevaPosicion))

    def mover(self) -> None:
        self.movimientos += 1
        posibleMovimiento = self.model.grid.get_neighborhood(
            self.pos,
            moore=False,
            include_center=False)

        nuevaPosicion = self.random.choice(posibleMovimiento)
        self.model.grid.move_agent(self, nuevaPosicion)

    def step(self) -> None:
        if (self.pos in self.model.cajasLista):
            self.cargar(self.pos)

        elif(self.pos in self.model.repisasLista):
            self.revisarRepisa(self.pos)

        if(self.carga is True):
            self.moverCargado()

        else:
            self.mover()


class Cajas(mesa.Agent):
    def __init__(self, unique_id: str, model: mesa.Model) -> None:
        super().__init__(unique_id, model)
        self.cargada = False
        self.movimientos = 0
        self.type = "Caja"


class Repisas(mesa.Agent):
    def __init__(self, unique_id: str, model: mesa.Model) -> None:
        super().__init__(unique_id, model)
        self.tiddy = 0
        self.empty = True
        self.type = "Repisa"
        self.movimientos = 0


class AlmacenModel(mesa.Model):
    def __init__(self, N, width, height, numCajas, time) -> None:

        self.numAgents = N
        self.grid = mesa.space.MultiGrid(width, height, True)
        self.cells = width * height
        self.repisasLista = []
        self.cajasLista = []
        self.numCajas = numCajas
        self.repisa = numCajas // 5
        self.schedule = mesa.time.SimultaneousActivation(self)
        self.time = time
        self.running = True

        print(self.repisa)

        for i in range(self.numAgents):
            aspiradora = Walle("Robot Cargador " + str(i), self)
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            self.schedule.add(aspiradora)
            self.grid.place_agent(aspiradora, (x, y))

        bloques = self.numCajas
        while(bloques > 0):
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            temp = (x, y)

            if (temp not in self.cajasLista and temp not in self.repisasLista):
                self.cajasLista.append(temp)
                caja = Cajas("Caja " + str(bloques), self)
                self.grid.place_agent(caja, (temp))
                self.schedule.add(caja)
                bloques -= 1

        while(self.repisa > 0):
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            pos = (x, y)

            if (pos not in self.repisasLista and pos not in self.cajasLista):
                self.repisasLista.append(pos)
                estanteria = Repisas("Repisa " + str(self.repisa), self)
                self.schedule.add(estanteria)
                self.grid.place_agent(estanteria, (pos))
                self.repisa -= 1

        self.datacollector = mesa.DataCollector(
            model_reporters={"Movimientos": movimientos_agentes,
                             "Tiempo": calc_tiempo})

    def step(self) -> None:

        self.datacollector.collect(self)
        self.schedule.step()
