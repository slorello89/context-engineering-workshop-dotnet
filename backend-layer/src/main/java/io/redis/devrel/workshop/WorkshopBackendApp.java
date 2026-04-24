package io.redis.devrel.workshop;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.scheduling.annotation.EnableScheduling;

@SpringBootApplication
@EnableScheduling
public class WorkshopBackendApp {

	public static void main(String[] args) {
		SpringApplication.run(WorkshopBackendApp.class, args);
	}

}
