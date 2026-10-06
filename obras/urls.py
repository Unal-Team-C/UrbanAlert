from rest_framework.routers import DefaultRouter

from .obras_viewset import ObraViewSet


router = DefaultRouter()
router.register("obras", ObraViewSet, basename="obras")

urlpatterns = router.urls