from rest_framework.exceptions import ValidationError

from obras.models import Obra


def obtener_obra(id):

    try: obra = Obra.objects.get(id=id)
    except: raise ValidationError({"id":"No existe una obra con el id."})
    return obra

def actualizar_avance_obra(obra,nuevo_avance):

    obra.avance = nuevo_avance
    obra.save()

    return obra

def actualizar_estado_obra(obra,nuevo_estado):

    obra.estado = nuevo_estado
    obra.save()

    return obra