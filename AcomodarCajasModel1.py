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
        self.vecindario = []

    def revisarRepisa(self, cell) -> None:
        cellmates = self.model.grid.get_cell_list_contents([cell])
        for repisa in cellmates:
            if(repisa.type == "Repisa"):
                if self.carga is True:
                    if repisa.tiddy < 5:
                        repisa.tiddy += 1
                        self.dejar()
                    elif repisa.tiddy == 5:
                        repisa.type = "RepisaLLena"
                        self.model.repisasLista.remove(repisa.pos)

    def dejar(self) -> None:
        self.carga = False
        self.cajaCargada = None
        self.type = "Robot"

    def cargar(self, cell) -> None:
        cellmates = self.model.grid.get_cell_list_contents([cell])
        for caja in cellmates:
            if(caja.type == "Caja"):
                self.carga = True
                self.cajaCargada = caja
                caja.cargada = True
                self.type = "robotCargando"
        self.model.cajasLista.remove(cell)
        self.model.numCajas -= 1

    def moverCargado(self):
        x = self.pos[0] - self.model.repisasLista[0][0]
        y = self.pos[1] - self.model.repisasLista[0][1]

        if x > 0:
            nuevaPosicion = (self.pos[0] - 1, self.pos[1])
            self.model.grid.move_agent(self, nuevaPosicion)
            self.model.grid.move_agent(self.cajaCargada, (nuevaPosicion))
            self.movimientos += 1
        elif y > 0:
            nuevaPosicion = (self.pos[0], self.pos[1] - 1)
            self.model.grid.move_agent(self, nuevaPosicion)
            self.model.grid.move_agent(self.cajaCargada, (nuevaPosicion))
            self.movimientos += 1
        elif x < 0:
            nuevaPosicion = (self.pos[0] + 1, self.pos[1])
            self.model.grid.move_agent(self, nuevaPosicion)
            self.model.grid.move_agent(self.cajaCargada, (nuevaPosicion))
            self.movimientos += 1
        elif y < 0:
            nuevaPosicion = (self.pos[0], self.pos[1] + 1)
            self.model.grid.move_agent(self, nuevaPosicion)
            self.model.grid.move_agent(self.cajaCargada, (nuevaPosicion))
            self.movimientos += 1

    def mover(self) -> None:
        self.vecindario = []
        self.movimientos += 1
        posibleMovimiento = self.model.grid.get_neighborhood(
            self.pos,
            moore=False,
            include_center=False)
        for position in posibleMovimiento:
            casilla = self.model.grid.is_cell_empty(position)
            if casilla is False and position not in self.model.repisasLista:
                self.vecindario.append(position)
        if len(self.vecindario) != 0:
            for pos in self.vecindario:
                objetosNuevaPos = self.model.grid.get_cell_list_contents([pos])
                for objeto in objetosNuevaPos:
                    if objeto.type == "Caja":
                        self.model.grid.move_agent(self, pos)
                    else:
                        nuevaPosicion = self.random.choice(posibleMovimiento)
                        self.model.grid.move_agent(self, nuevaPosicion)
        else:
            nuevaPosicion = self.random.choice(posibleMovimiento)
            self.model.grid.move_agent(self, nuevaPosicion)


    def step(self) -> None:
        if (self.pos in self.model.cajasLista and self.carga is False):
            self.cargar(self.pos)

        elif(self.pos in self.model.repisasLista):
            self.revisarRepisa(self.pos)

        if(self.carga is True):
            self.moverCargado()

        else:
            if(self.model.numCajas > 0):
                self.mover()
            elif(self.model.numCajas == 0 and self.carga is False):
                self.model.grid.move_agent(self, self.pos)


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

    def step(self):
        """
        Each step, get the number of boxes in its cell position
        """
        self.get_number_of_boxes_in_stack()

    def get_number_of_boxes_in_stack(self):
        """
        Get the number of boxes in the stack
        Returns:
            int: the number of boxes in the stack
        """
        cantidad = 0
        for num in self.model.grid.get_cell_list_contents([self.pos]):
            if num.type == "Caja":
                cantidad += 1
        return cantidad


class AlmacenModel(mesa.Model):
    def __init__(self, N, width, height, numCajas, time) -> None:

        self.numAgents = N
        self.grid = mesa.space.MultiGrid(width, height, True)
        self.cells = width * height
        self.repisasLista: list[tuple] = []
        self.cajasLista: list[tuple] = []
        self.walleLista: list[tuple] = []
        self.cajasNoAcomodadas: list[tuple] = []
        self.numCajas = numCajas
        self.repisa = numCajas // 5
        self.schedule = mesa.time.SimultaneousActivation(self)
        self.time = time
        self.running = True

        robots = self.numAgents
        while(robots > 0):
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            pos = (x, y)

            if (pos not in self.repisasLista and pos not in
                self.cajasLista and pos not in
               self.walleLista):
                self.walleLista.append(pos)
                walle = Walle("Robot Cargador " + str(robots), self)
                self.schedule.add(walle)
                self.grid.place_agent(walle, (x, y))
                robots -= 1

        bloques = self.numCajas
        while(bloques > 0):
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            temp = (x, y)

            if (temp not in self.cajasLista and temp not in
                self.repisasLista and temp not in
               self.walleLista):
                self.cajasLista.append(temp)
                caja = Cajas("Caja " + str(bloques), self)
                self.grid.place_agent(caja, (temp))
                self.schedule.add(caja)
                bloques -= 1

        while(self.repisa > 0):
            x = self.random.randrange(self.grid.width)
            y = self.random.randrange(self.grid.height)
            pos = (x, y)

            if (pos not in self.repisasLista and pos not in
                self.cajasLista and pos not in
               self.walleLista):
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
