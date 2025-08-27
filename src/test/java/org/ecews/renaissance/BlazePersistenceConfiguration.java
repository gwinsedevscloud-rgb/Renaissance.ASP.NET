/*
* MIT License
*
* Copyright (c) 2025 [AUTHOR OR ORGANIZATION]
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
package org.ecews.renaissance;

import org.ecews.renaissance.domain.RenaissancePluginDomain;
import io.github.mattae.snl.core.api.domain.CoreDomain;
import com.blazebit.persistence.Criteria;
import com.blazebit.persistence.CriteriaBuilderFactory;
import com.blazebit.persistence.integration.view.spring.EnableEntityViews;
import com.blazebit.persistence.spi.CriteriaBuilderConfiguration;
import com.blazebit.persistence.view.ConfigurationProperties;
import com.blazebit.persistence.view.EntityViewManager;
import com.blazebit.persistence.view.spi.EntityViewConfiguration;
import io.github.mattae.snl.core.api.config.AuditViewListenersConfiguration;
import io.github.mattae.snl.core.api.services.TransactionHandler;
import org.springframework.beans.factory.config.ConfigurableBeanFactory;
import org.springframework.boot.autoconfigure.domain.EntityScan;
import org.springframework.context.annotation.*;

import jakarta.persistence.EntityManagerFactory;
import jakarta.persistence.PersistenceUnit;
import java.util.UUID;

@Configuration
@EntityScan(basePackageClasses = {CoreDomain.class, RenaissancePluginDomain.class})
@EnableEntityViews(basePackageClasses = {CoreDomain.class, RenaissancePluginDomain.class})
@Import(TransactionHandler.class)
public class BlazePersistenceConfiguration {
    
    @PersistenceUnit
    private EntityManagerFactory entityManagerFactory;
    
    @Bean
    @Scope(ConfigurableBeanFactory.SCOPE_SINGLETON)
    @Lazy(false)
    public CriteriaBuilderFactory createCriteriaBuilderFactory() {
        CriteriaBuilderConfiguration config = Criteria.getDefault();
        return config.createCriteriaBuilderFactory(entityManagerFactory);
    }
    
    @Bean
    @Scope(ConfigurableBeanFactory.SCOPE_SINGLETON)
    @Lazy(false)
    public EntityViewManager entityViewManager(CriteriaBuilderFactory cbf, EntityViewConfiguration entityViewConfiguration) {
        entityViewConfiguration.setProperty(ConfigurationProperties.UPDATER_STRICT_CASCADING_CHECK, "false");
        entityViewConfiguration.setProperty(ConfigurationProperties.UPDATER_FLUSH_MODE, "partial");
        entityViewConfiguration.setTypeTestValue(UUID.class, UUID.randomUUID());
        entityViewConfiguration.addEntityViewListener(AuditViewListenersConfiguration.class);
        return entityViewConfiguration.createEntityViewManager(cbf);
    }
}
