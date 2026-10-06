from django.db import transaction
from rest_framework.viewsets import ModelViewSet
from rest_framework.decorators import action
from rest_framework.response import Response

from obras.services.obras_services import actualizar_avance_obra, actualizar_estado_obra, obtener_obra

from .models import Obra
from .obras_serializers import ObraSerializer


class ObraViewSet(ModelViewSet):
    model = Obra
    queryset = Obra.objects.all()
    serializer_class = ObraSerializer

    http_method_names = ["get", "post"]

    @action(detail=False, methods=["post"], url_path="actualizar_avance")
    def actualizar_avance(self, request):

        data_avance = request.data
        serializer = self.serializer_class(data=data_avance,partial=True)
        serializer.is_valid(raise_exception=True)
        obra = obtener_obra(data_avance['id'])

        with transaction.atomic():
            obra = actualizar_avance_obra(obra,data_avance['avance'])

            return Response({'message': 'El avance de la obra ha sido actualizado correctamente.',
                            "nuevo_avance": obra.avance},status=200)

    @action(detail=False, methods=["post"], url_path="actualizar_estado")
    def actualizar_estado(self, request):

        data_estado = request.data
        print(data_estado)
        serializer = self.serializer_class(data=data_estado,partial=True)
        serializer.is_valid(raise_exception=True)
        obra = obtener_obra(data_estado['id'])

        with transaction.atomic():
            obra = actualizar_estado_obra(obra,data_estado['estado'])

            return Response({'message': 'El estado de la obra ha sido actualizado correctamente.',
                                "nuevo_estado": obra.estado},status=200)