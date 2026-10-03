package com.urbanalert.users.infrastructure.config;

import com.urbanalert.users.domain.model.Role;
import com.urbanalert.users.infrastructure.persistence.entity.UserJpaEntity;
import com.urbanalert.users.infrastructure.persistence.repository.UserJpaRepository;
import org.springframework.boot.ApplicationArguments;
import org.springframework.boot.ApplicationRunner;
import org.springframework.context.annotation.Profile;
import org.springframework.stereotype.Component;

import java.util.UUID;

@Component
@Profile("local")
public class DataInitializer implements ApplicationRunner {

    private static final UUID SEED_ADMIN_ID = UUID.fromString("018f4c2a-0000-7000-8000-000000000001");

    private final UserJpaRepository userJpaRepository;

    public DataInitializer(UserJpaRepository userJpaRepository) {
        this.userJpaRepository = userJpaRepository;
    }

    @Override
    public void run(ApplicationArguments args) {
        if (userJpaRepository.existsById(SEED_ADMIN_ID)) return;

        UserJpaEntity admin = new UserJpaEntity();
        admin.setId(SEED_ADMIN_ID);
        admin.setName("Administrator");
        admin.setEmail("admin@urbanalert.com");
        admin.setRole(Role.ADMIN);
        userJpaRepository.save(admin);
    }
}
