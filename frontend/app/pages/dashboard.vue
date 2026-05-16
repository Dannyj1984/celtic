<template>
  <div>
    <!-- Admin Dashboard -->
    <div v-if="isAdmin">
      <div class="mb-8">
        <h1 class="text-2xl font-bold text-text-tertiary">
          Welcome back, {{ user?.fullName?.split(' ')[0] }} 👋
        </h1>
        <p class="text-text-secondary mt-1">
          Here's your team overview
        </p>
      </div>

      <div class="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-4 mb-8">
        <NuxtLink to="/admin/players" class="card p-5 hover:border-celtic-green transition-colors group">
          <div class="flex flex-col h-full">
            <div class="p-2 bg-celtic-green/10 rounded-lg w-fit mb-3 group-hover:bg-celtic-green/20 transition-colors">
              <UserGroupIcon class="w-6 h-6 text-celtic-green" />
            </div>
            <p class="text-white text-sm font-medium uppercase tracking-wide">Squad</p>
            <p class="text-xl font-bold text-text-tertiary mt-1">Players &rarr;</p>
          </div>
        </NuxtLink>

        <NuxtLink to="/admin/schedule" class="card p-5 hover:border-celtic-green transition-colors group">
          <div class="flex flex-col h-full">
            <div class="p-2 bg-blue-500/10 rounded-lg w-fit mb-3 group-hover:bg-blue-500/20 transition-colors">
              <CalendarDaysIcon class="w-6 h-6 text-blue-500" />
            </div>
            <p class="text-white text-sm font-medium uppercase tracking-wide">Schedule</p>
            <p class="text-xl font-bold text-text-tertiary mt-1">Events &rarr;</p>
          </div>
        </NuxtLink>

        <NuxtLink to="/admin/matches" class="card p-5 hover:border-celtic-green transition-colors group">
          <div class="flex flex-col h-full">
            <div class="p-2 bg-celtic-gold/10 rounded-lg w-fit mb-3 group-hover:bg-celtic-gold/20 transition-colors">
              <TrophyIcon class="w-6 h-6 text-celtic-gold" />
            </div>
            <p class="text-white text-sm font-medium uppercase tracking-wide">Matches</p>
            <p class="text-xl font-bold text-text-tertiary mt-1">Results &rarr;</p>
          </div>
        </NuxtLink>

        <NuxtLink to="/admin/seasons" class="card p-5 hover:border-celtic-green transition-colors group">
          <div class="flex flex-col h-full">
            <div class="p-2 bg-purple-500/10 rounded-lg w-fit mb-3 group-hover:bg-purple-500/20 transition-colors">
              <ChartBarIcon class="w-6 h-6 text-purple-500" />
            </div>
            <p class="text-white text-sm font-medium uppercase tracking-wide">Seasons</p>
            <p class="text-xl font-bold text-text-tertiary mt-1">History &rarr;</p>
          </div>
        </NuxtLink>

        <NuxtLink to="/admin/parents" class="card p-5 hover:border-celtic-green transition-colors group">
          <div class="flex flex-col h-full">
            <div class="p-2 bg-orange-500/10 rounded-lg w-fit mb-3 group-hover:bg-orange-500/20 transition-colors">
              <UsersIcon class="w-6 h-6 text-orange-500" />
            </div>
            <p class="text-white text-sm font-medium uppercase tracking-wide">Parents</p>
            <p class="text-xl font-bold text-text-tertiary mt-1">Contacts &rarr;</p>
          </div>
        </NuxtLink>

        <NuxtLink to="/admin/settings" class="card p-5 hover:border-celtic-green transition-colors group">
          <div class="flex flex-col h-full">
            <div class="p-2 bg-gray-500/10 rounded-lg w-fit mb-3 group-hover:bg-gray-500/20 transition-colors">
              <Cog6ToothIcon class="w-6 h-6 text-gray-500" />
            </div>
            <p class="text-white text-sm font-medium uppercase tracking-wide">Settings</p>
            <p class="text-xl font-bold text-text-tertiary mt-1">Club &rarr;</p>
          </div>
        </NuxtLink>

        <button @click="showAnnouncementModal"
          class="card p-5 hover:border-celtic-green transition-colors group text-left">
          <div class="flex flex-col h-full">
            <div class="p-2 bg-pink-500/10 rounded-lg w-fit mb-3 group-hover:bg-pink-500/20 transition-colors">
              <MegaphoneIcon class="w-6 h-6 text-pink-500" />
            </div>
            <p class="text-white text-sm font-medium uppercase tracking-wide">Push</p>
            <p class="text-xl font-bold text-text-tertiary mt-1">Announce &rarr;</p>
          </div>
        </button>

        <NuxtLink to="/admin/players"
          class="card p-5 hover:border-celtic-green transition-colors border-dashed border-celtic-green/50 group bg-celtic-green/5">
          <div class="flex flex-col h-full justify-center items-center text-center">
            <PlusCircleIcon class="w-8 h-8 text-celtic-green mb-2 group-hover:scale-110 transition-transform" />
            <p class="text-lg font-bold text-celtic-green">Add Player</p>
          </div>
        </NuxtLink>
      </div>
    </div>

    <!-- Parent Dashboard -->
    <div v-else>
      <div v-if="pending" class="text-center py-10">
        <p class="text-text-muted">Loading dashboard...</p>
      </div>
      <div v-else-if="error" class="text-center py-10">
        <p class="text-red-500">Failed to load dashboard. Please try again.</p>
      </div>
      <div v-else-if="dashboardData">
        <!-- Welcome Section -->
        <div class="mb-8 flex justify-between items-center">
          <div>
            <h1 class="text-3xl font-bold text-text-primary">
              Welcome, {{ dashboardData.parentName.split(' ')[0] }}
            </h1>
            <p class="text-text-secondary mt-1 text-lg">
              {{ dashboardData.playerName }}
            </p>
          </div>
          <div class="flex flex-col items-center gap-3">
            <UButton :color="isSubscribed ? 'green' : 'blue'" :variant="isSubscribed ? 'soft' : 'solid'"
              @click="toggleNotifications" :loading="notificationLoading">
              <template #leading>
                <component :is="isSubscribed ? BellSlashIcon : BellIcon" class="w-5 h-5" />
              </template>
              {{ isSubscribed ? 'Notifications Enabled' : 'Enable Notifications' }}
            </UButton>
            <UButton color="blue" variant="solid" @click="changePassword">Change Password</UButton>
          </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-12 gap-6">
          <!-- Left Column -->
          <div class="lg:col-span-8 space-y-6">

            <!-- Subscription Status -->
            <UCard class="bg-bg-card border border-border-color shadow-sm rounded-xl overflow-hidden">
              <div class="flex items-center justify-between">
                <div>
                  <h3 class="text-sm font-semibold text-text-muted uppercase tracking-wider mb-1">Subscription Status
                  </h3>
                  <div class="flex items-center gap-3">
                    <span class="text-xl font-bold text-text-primary">{{ dashboardData.subscriptionStatus }}</span>
                    <UBadge v-if="dashboardData.subscriptionStatus === 'Active'" color="green" variant="subtle"
                      size="sm">Paid
                    </UBadge>
                    <UBadge v-else-if="dashboardData.subscriptionStatus === 'Payment Due'" color="red" variant="subtle"
                      size="sm">Due</UBadge>
                    <UBadge v-else color="gray" variant="subtle" size="sm">Inactive</UBadge>
                  </div>
                </div>
                <div class="text-right" v-if="dashboardData.nextSubPaymentDate">
                  <p class="text-xs text-text-muted mb-1">Next Payment</p>
                  <p class="text-sm font-medium text-text-primary">
                    {{ new Date(dashboardData.nextSubPaymentDate).toDateString() }}
                  </p>
                </div>
              </div>
            </UCard>

            <!-- Upcoming Activities -->
            <div>
              <h2 class="text-xl font-bold text-text-primary mb-4">Upcoming Activities</h2>
              <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                <!-- Next Match -->
                <UCard
                  class="bg-bg-card border border-border-color shadow-sm hover:border-celtic-green transition-colors">
                  <div class="flex justify-between items-start mb-4">
                    <div class="p-2 bg-celtic-green/10 rounded-lg">
                      <CalendarDaysIcon class="w-6 h-6 text-celtic-green" />
                    </div>
                    <UBadge color="primary" variant="subtle">Match</UBadge>
                  </div>
                  <h3 class="text-lg font-bold text-text-primary mb-1">vs {{ dashboardData.nextMatch?.opposition ||
                    'TBD' }}</h3>
                  <p class="text-sm text-text-muted mb-2">
                    {{ dashboardData.nextMatch ? new Date(dashboardData.nextMatch.date).toLocaleDateString() :
                      'Noupcoming matches' }}
                  </p>
                  <div class="flex justify-between items-center">
                    <p v-if="dashboardData.nextMatch?.location"
                      class="text-sm text-text-secondary flex items-center gap-1">
                      <MapPinIcon class="w-4 h-4" />
                      {{ dashboardData.nextMatch.location }}
                    </p>
                    <div class="flex items-center gap-1">
                      <p v-if="dashboardData.attendingNextMatch"
                        class="text-sm text-text-secondary flex items-center gap-1">
                        <CheckCircleIcon class="w-8 h-8 text-celtic-green" />
                      </p>
                      <UButton v-if="dashboardData.nextMatch?.location" color="gray" variant="ghost" size="xs" square
                        @click="openInMaps(dashboardData.nextMatch.location)">
                        <MapPinIcon class="w-4 h-4" />
                      </UButton>
                      <UDropdown v-if="dashboardData.nextMatch" :items="matchCalendarMenuItems"
                        :popper="{ placement: 'bottom-end' }">
                        <UButton color="gray" variant="ghost" size="xs" square>
                          <CalendarDaysIcon class="w-4 h-4" />
                        </UButton>
                        <template #item="{ item }">
                          <component :is="item.icon" class="w-4 h-4 mr-2"
                            v-if="item.icon && typeof item.icon !== 'string'" />
                          <UIcon :name="item.icon" class="w-4 h-4 mr-2" v-else-if="item.icon" />
                          <span>{{ item.label }}</span>
                        </template>
                      </UDropdown>
                    </div>
                  </div>
                </UCard>

                <!-- Training -->
                <UCard
                  class="bg-bg-card border border-border-color shadow-sm hover:border-celtic-green transition-colors">
                  <div class="flex justify-between items-start mb-4">
                    <div class="p-2 bg-blue-500/10 rounded-lg">
                      <BoltIcon class="w-6 h-6 text-blue-500" />
                    </div>
                    <UBadge color="blue" variant="subtle">Training</UBadge>
                  </div>
                  <h3 class="text-lg font-bold text-text-primary mb-1">Weekly Training</h3>
                  <p v-if="dashboardData.trainingSchedule" class="text-sm text-text-muted mb-2">
                    {{ dashboardData.trainingSchedule.day }}s, {{ dashboardData.trainingSchedule.startTime }} - {{
                      dashboardData.trainingSchedule.endTime }}
                  </p>
                  <p v-else class="text-sm text-text-muted mb-2">Not scheduled</p>
                  <div class="flex justify-between">
                    <p v-if="dashboardData.trainingSchedule?.location"
                      class="text-sm text-text-secondary flex items-center gap-1">
                      <MapPinIcon class="w-4 h-4" />
                      {{ dashboardData.trainingSchedule.location }}
                    </p>
                    <p v-if="dashboardData.attendingNextTraining"
                      class="text-sm text-text-secondary flex items-center gap-1">
                      <CheckCircleIcon class="w-8 h-8 text-celtic-green" />
                    </p>
                  </div>

                  <div v-if="dashboardData.trainingSchedule?.trainingFocus"
                    class="mt-4 p-3 bg-celtic-green/5 border border-celtic-green/10 rounded-lg">
                    <p class="text-[10px] font-bold text-celtic-green uppercase tracking-widest mb-1">Session Focus</p>
                    <p class="text-sm text-text-primary leading-tight font-medium italic">
                      "{{ dashboardData.trainingSchedule.trainingFocus }}"
                    </p>
                  </div>
                </UCard>
              </div>
            </div>

            <!-- Parent Quick Actions -->
            <div>
              <h2 class="text-xl font-bold text-text-primary mb-4">Parent Quick Actions</h2>
              <div class="grid grid-cols-2 sm:grid-cols-3 gap-3">
                <UButton color="gray" variant="solid"
                  class="flex flex-col items-center justify-center h-24 gap-2 !bg-bg-card border border-border-color hover:border-celtic-green hover:!bg-celtic-green/5 transition-all"
                  @click="registerForTraining" :loading="pending">
                  <CheckCircleIcon class="w-6 h-6 text-celtic-green" />
                  <span class="text-xs font-medium text-center whitespace-normal">Register for Training</span>
                </UButton>
                <UButton color="gray" variant="solid"
                  class="flex flex-col items-center justify-center h-24 gap-2 !bg-bg-card border border-border-color hover:border-blue-500 hover:!bg-blue-500/5 transition-all"
                  @click="confirmMatch" :loading="pending">
                  <CalendarDaysIcon class="w-6 h-6 text-blue-500" />
                  <span class="text-xs font-medium text-center whitespace-normal">Confirm Availability</span>
                </UButton>
              </div>
            </div>

            <!-- Coach Notes -->
            <div v-if="dashboardData.coachNotes">
              <h2 class="text-xl font-bold text-text-primary mb-4">Coach's Notes</h2>
              <UCard class="bg-celtic-green/5 border border-celtic-green/20 shadow-sm relative overflow-hidden">
                <div class="absolute top-0 right-0 p-4 opacity-10">
                  <ChatBubbleBottomCenterTextIcon class="w-24 h-24 text-celtic-green" />
                </div>
                <div class="relative">
                  <p class="text-text-primary leading-relaxed whitespace-pre-line italic">
                    "{{ dashboardData.coachNotes }}"
                  </p>
                  <div class="mt-4 flex items-center gap-2">
                    <div
                      class="w-8 h-8 rounded-full bg-celtic-green flex items-center justify-center text-white font-bold text-xs">
                      C
                    </div>
                    <span class="text-sm font-medium text-text-secondary">Danny</span>
                  </div>
                </div>
              </UCard>
            </div>
          </div>

          <!-- Right Column -->
          <div class="lg:col-span-4 space-y-6">

            <!-- Season Performance -->
            <NuxtLink to="/season" class="block">
              <UCard
                class="bg-bg-card border border-border-color shadow-sm hover:border-celtic-green transition-colors">
                <h3 class="text-lg font-bold text-text-primary mb-6">Season Performance</h3>
                <div class="grid grid-cols-2 gap-4">
                  <div class="flex flex-col items-center">
                    <div class="relative w-24 h-24 mb-2">
                      <svg class="w-full h-full transform -rotate-90" viewBox="0 0 36 36">
                        <path class="text-border-color" stroke-width="3" stroke="currentColor" fill="none"
                          d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                        <path class="text-celtic-green transition-all duration-1000 ease-out" stroke-width="3"
                          :stroke-dasharray="trainingPercentage + ', 100'" stroke="currentColor" fill="none"
                          d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                      </svg>
                      <div class="absolute inset-0 flex items-center justify-center">
                        <span class="text-lg font-bold text-text-primary">{{ trainingPercentage }}%</span>
                      </div>
                    </div>
                    <span class="text-xs font-medium text-text-muted uppercase">Training</span>
                    <p class="text-[10px] text-text-secondary mt-1">
                      {{ dashboardData.performance?.training?.attendedSessions || 0 }} / {{
                        dashboardData.performance?.training?.totalSessions || 0 }}
                    </p>
                  </div>

                  <div class="flex flex-col items-center">
                    <div class="relative w-24 h-24 mb-2">
                      <svg class="w-full h-full transform -rotate-90" viewBox="0 0 36 36">
                        <path class="text-border-color" stroke-width="3" stroke="currentColor" fill="none"
                          d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                        <path class="text-celtic-gold transition-all duration-1000 ease-out" stroke-width="3"
                          :stroke-dasharray="matchPercentage + ', 100'" stroke="currentColor" fill="none"
                          d="M18 2.0845 a 15.9155 15.9155 0 0 1 0 31.831 a 15.9155 15.9155 0 0 1 0 -31.831" />
                      </svg>
                      <div class="absolute inset-0 flex items-center justify-center">
                        <span class="text-lg font-bold text-text-primary">{{ matchPercentage }}%</span>
                      </div>
                    </div>
                    <span class="text-xs font-medium text-text-muted uppercase">Matches</span>
                    <p class="text-[10px] text-text-secondary mt-1">
                      {{ dashboardData.performance?.matches?.attendedSessions || 0 }} / {{
                        dashboardData.performance?.matches?.totalSessions || 0 }}
                    </p>
                  </div>
                </div>
                <p class="text-center text-xs text-text-secondary mt-6 pt-4 border-t border-border/50">
                  Performance tracking since Sep 1st
                </p>
              </UCard>
            </NuxtLink>

            <UCard class="bg-bg-card border border-border-color shadow-sm hover:border-celtic-green transition-colors">
              <h3 class="text-lg font-bold text-text-primary mb-6">Good to Know</h3>
              <p class="text-sm text-text-secondary">{{ dashboardData.trainingSchedule.goodToKnow }}</p>
            </UCard>

          </div>
        </div>
      </div>

      <!-- WhatsApp FAB (Mobile Only) -->
      <a v-if="dashboardData?.coachWhatsAppNumber" :href="'https://wa.me/' + dashboardData.coachWhatsAppNumber"
        target="_blank" rel="noopener noreferrer"
        class="fixed bottom-24 right-6 md:hidden w-14 h-14 bg-[#25D366] rounded-full flex items-center justify-center shadow-[0_4px_14px_0_rgba(37,211,102,0.39)] hover:scale-110 transition-all z-50">
        <svg xmlns="http://www.w3.org/2000/svg" width="30" height="30" viewBox="0 0 24 24" fill="white">
          <path
            d="M.057 24l1.687-6.163c-1.041-1.804-1.588-3.849-1.587-5.946.003-6.556 5.338-11.891 11.893-11.891 3.181.001 6.167 1.24 8.413 3.488 2.245 2.248 3.481 5.236 3.48 8.414-.003 6.557-5.338 11.892-11.893 11.892-1.99-.001-3.951-.5-5.688-1.448l-6.305 1.654zm6.597-3.807c1.676.995 3.276 1.591 5.392 1.592 5.448 0 9.886-4.434 9.889-9.885.002-5.462-4.415-9.89-9.881-9.892-5.452 0-9.887 4.434-9.889 9.884-.001 2.225.651 3.891 1.746 5.634l-.999 3.648 3.742-.981zm11.387-5.464c-.074-.124-.272-.198-.57-.347-.297-.149-1.758-.868-2.031-.967-.272-.099-.47-.149-.669.149-.198.297-.768.967-.941 1.165-.173.198-.347.223-.644.074-.297-.149-1.255-.462-2.39-1.475-.883-.788-1.48-1.761-1.653-2.059-.173-.297-.018-.458.13-.606.134-.133.297-.347.446-.521.149-.174.198-.298.297-.497.099-.198.05-.372-.025-.521-.075-.149-.669-1.612-.916-2.207-.242-.579-.487-.501-.669-.51l-.57-.01c-.198 0-.52.074-.792.372s-1.04 1.016-1.04 2.479 1.065 2.876 1.213 3.074c.149.198 2.095 3.2 5.076 4.487.709.306 1.263.489 1.694.625.712.216 1.36.186 1.871.11.57-.085 1.758-.719 2.006-1.413.248-.694.248-1.289.173-1.413z" />
        </svg>
      </a>

      <!-- RSVP Bulk Modal -->
      <UModal v-model="isRsvpModalOpen">
        <UCard :ui="{ ring: '', divide: 'divide-y divide-gray-100 dark:divide-gray-800' }">
          <template #header>
            <div class="flex items-center justify-between">
              <h3 class="text-lg font-bold text-text-primary">
                {{ rsvpType === 'Training' ? 'Training Attendance' : 'Match Availability' }}
              </h3>
              <UButton color="gray" variant="ghost" icon="i-heroicons-x-mark-20-solid" class="-my-1"
                @click="isRsvpModalOpen = false" />
            </div>
          </template>

          <div class="p-4 space-y-4">
            <p class="text-sm text-text-secondary">Select all sessions you will be attending for the next 4 weeks.</p>

            <div v-if="eventsLoading" class="flex justify-center py-8">
              <div class="animate-spin w-6 h-6 rounded-full border-2 border-celtic-green border-t-transparent"></div>
            </div>

            <div v-else class="space-y-3">
              <div v-for="event in upcomingEvents" :key="event.id"
                class="flex items-center justify-between p-3 rounded-lg border border-border-color hover:bg-surface-hover transition-colors">
                <div class="flex flex-col">
                  <span class="text-sm font-bold text-text-primary">
                    {{ new Date(event.dateTime).toLocaleDateString('en-GB', {
                      weekday: 'long', day: 'numeric', month:
                        'short'
                    })
                    }}
                  </span>
                  <span class="text-xs text-text-muted">
                    {{ new Date(event.dateTime).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' }) }}
                    <span v-if="event.opposition">vs {{ event.opposition }}</span>
                  </span>
                </div>
                <UCheckbox v-model="selectedRsvps[event.id]" color="green" />
              </div>

              <div v-if="upcomingEvents.length === 0" class="text-center py-4 text-text-muted">
                No upcoming {{ rsvpType?.toLowerCase() }} found for the next 4 weeks.
              </div>
            </div>
          </div>

          <template #footer>
            <div class="flex justify-end gap-3">
              <UButton color="gray" variant="ghost" @click="isRsvpModalOpen = false">Cancel</UButton>
              <UButton color="green" @click="submitBulkRsvp" :loading="submittingRsvp"
                :disabled="upcomingEvents.length === 0">
                Save Attendance
              </UButton>
            </div>
          </template>
        </UCard>
      </UModal>

      <!-- Change Password Modal -->
      <UModal v-model="isChangePasswordModalOpen">
        <UCard :ui="{ ring: '', divide: 'divide-y divide-gray-100 dark:divide-gray-800' }">
          <template #header>
            <div class="flex items-center justify-between">
              <h3 class="text-base font-semibold leading-6 text-gray-900 dark:text-white">
                Change Password
              </h3>
              <UButton color="gray" variant="ghost" icon="i-heroicons-x-mark-20-solid" class="-my-1"
                @click="isChangePasswordModalOpen = false" />
            </div>
          </template>

          <form @submit.prevent="submitChangePassword" class="space-y-4" data-testid="change-password-form">
            <UFormGroup label="Current Password" required>
              <UInput v-model="passwordForm.currentPassword" type="password" required />
            </UFormGroup>
            <UFormGroup label="New Password" required>
              <UInput v-model="passwordForm.newPassword" type="password" required minlength="6" />
            </UFormGroup>
            <UFormGroup label="Confirm New Password" required>
              <UInput v-model="passwordForm.confirmPassword" type="password" required minlength="6" />
            </UFormGroup>

            <div class="flex justify-end gap-3 mt-6">
              <UButton color="gray" variant="ghost" @click="isChangePasswordModalOpen = false">Cancel</UButton>
              <UButton type="submit" color="blue" :loading="changingPassword">Update Password</UButton>
            </div>
          </form>
        </UCard>
      </UModal>


    </div>
    <UModal v-model="isAnnouncementModalOpen">
      <UCard :ui="{ ring: '', divide: 'divide-y divide-gray-100 dark:divide-gray-800' }">
        <template #header>
          <div class="flex items-center justify-between">
            <h3 class="text-base font-semibold leading-6 text-gray-900 dark:text-white">
              Send Team Announcement
            </h3>
            <UButton color="gray" variant="ghost" class="-my-1" @click="isAnnouncementModalOpen = false">
              <template #leading>
                <XMarkIcon class="w-5 h-5" />
              </template>
            </UButton>
          </div>
        </template>

        <form @submit.prevent="sendAnnouncement" class="space-y-4">
          <UFormGroup label="Title" required>
            <UInput v-model="announcementForm.title" placeholder="e.g. Training Update" required />
          </UFormGroup>
          <UFormGroup label="Message" required>
            <UTextarea v-model="announcementForm.message" placeholder="Type your message here..." required />
          </UFormGroup>
          <UFormGroup label="Link (optional)">
            <UInput v-model="announcementForm.url" placeholder="/admin/schedule" />
          </UFormGroup>

          <div class="flex justify-end gap-3 mt-6">
            <UButton color="gray" variant="ghost" @click="isAnnouncementModalOpen = false">Cancel</UButton>
            <UButton color="pink" type="submit" :loading="sendingAnnouncement">
              <template #leading>
                <PaperAirplaneIcon class="w-5 h-5" />
              </template>
              Send to All
            </UButton>
          </div>
        </form>
      </UCard>
    </UModal>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, reactive, onMounted } from 'vue'
import type { IDashboardData } from '~/interfaces/Dashboard'
import { useNotifications } from '~/composables/useNotifications'
import { useCalendar } from '~/composables/useCalendar'
import {
  MegaphoneIcon,
  PlusCircleIcon,
  XMarkIcon,
  PaperAirplaneIcon,
  CalendarDaysIcon,
  UserGroupIcon,
  TrophyIcon,
  ChartBarIcon,
  UsersIcon,
  Cog6ToothIcon,
  BoltIcon,
  MapPinIcon,
  CheckCircleIcon,
  ChatBubbleBottomCenterTextIcon,
  BellIcon,
  BellSlashIcon,
  GlobeAltIcon,
  ArrowDownTrayIcon
} from '@heroicons/vue/24/solid'

definePageMeta({
  layout: 'app',
})

useHead({
  title: 'Dashboard - Celtic FC',
  meta: [
    { name: 'description', content: 'Celtic FC team management dashboard' },
  ],
})

const { user, isAdmin, getAuthHeaders } = useAuth()
const { isSubscribed, subscribe, unsubscribe, checkSubscription, loading: notificationLoading } = useNotifications()
const { downloadIcs, openGoogleCalendar } = useCalendar()

onMounted(() => {
  checkSubscription()
})

const toggleNotifications = async () => {
  if (isSubscribed.value) {
    await unsubscribe()
    toast.add({ title: 'Notifications Disabled', description: 'You will no longer receive push alerts.', color: 'gray' })
  } else {
    try {
      await subscribe()
      toast.add({ title: 'Success', description: 'Notifications enabled!', color: 'green' })
    } catch (e: any) {
      toast.add({ title: 'Error', description: e.message || 'Failed to enable notifications.', color: 'red' })
    }
  }
}

// Fetch dashboard data
const { data: dashboardData, pending, error, refresh: refreshDashboard } = useFetch<IDashboardData>('/api/parent/dashboard', {
  key: 'parent-dashboard',
  server: false, // only fetch on client
  immediate: !isAdmin.value,
  headers: getAuthHeaders()
})

const trainingPercentage = computed(() => {
  if (!dashboardData.value?.performance?.training?.totalSessions) return 0;
  const { attendedSessions, totalSessions } = dashboardData.value.performance.training;
  return Math.round((attendedSessions / totalSessions) * 100);
})

const matchPercentage = computed(() => {
  if (!dashboardData.value?.performance?.matches?.totalSessions) return 0;
  const { attendedSessions, totalSessions } = dashboardData.value.performance.matches;
  return Math.round((attendedSessions / totalSessions) * 100);
})

const toast = useToast()

const getMatchCalendarEvent = () => {
  if (!dashboardData.value?.nextMatch) return null
  const match = dashboardData.value.nextMatch
  return {
    title: `Celtic FC vs ${match.opposition || 'TBD'}`,
    dateTime: match.date,
    location: match.location,
    description: `Match: Celtic FC vs ${match.opposition || 'TBD'}`
  }
}

const matchCalendarMenuItems = computed(() => [[
  {
    label: 'Google Calendar',
    icon: GlobeAltIcon,
    click: () => {
      const event = getMatchCalendarEvent()
      if (event) openGoogleCalendar(event)
    }
  },
  {
    label: 'Download .ics',
    icon: ArrowDownTrayIcon,
    click: () => {
      const event = getMatchCalendarEvent()
      if (event) downloadIcs(event)
    }
  }
]])

const openInMaps = (location: string) => {
  window.open(`https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(location)}`, '_blank')
}

// Announcement Logic
const isAnnouncementModalOpen = ref(false)
const sendingAnnouncement = ref(false)
const announcementForm = reactive({
  title: '',
  message: '',
  url: ''
})

const showAnnouncementModal = () => {
  if (!isAdmin.value) return
  announcementForm.title = ''
  announcementForm.message = ''
  announcementForm.url = ''
  isAnnouncementModalOpen.value = true
}

const sendAnnouncement = async () => {
  sendingAnnouncement.value = true
  try {
    await $fetch('/api/notifications/send-test', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: {
        ...announcementForm,
        toAll: true
      }
    })
    toast.add({ title: 'Success', description: 'Announcement sent to all devices!', color: 'green' })
    isAnnouncementModalOpen.value = false
    announcementForm.title = ''
    announcementForm.message = ''
    announcementForm.url = ''
  } catch (e) {
    toast.add({ title: 'Error', description: 'Failed to send announcement.', color: 'red' })
  } finally {
    sendingAnnouncement.value = false
  }
}

// RSVP Logic
const isRsvpModalOpen = ref(false)
const rsvpType = ref<'Training' | 'Match' | null>(null)
const upcomingEvents = ref<any[]>([])
const eventsLoading = ref(false)
const submittingRsvp = ref(false)
const selectedRsvps = reactive<Record<string, boolean>>({})

const registerForTraining = () => openRsvpModal('Training')
const confirmMatch = () => openRsvpModal('Match')

const openRsvpModal = async (type: 'Training' | 'Match') => {
  rsvpType.value = type
  isRsvpModalOpen.value = true
  eventsLoading.value = true

  try {
    const data = await $fetch<any[]>(`/api/parent/upcoming/${type}`, {
      headers: getAuthHeaders()
    })
    upcomingEvents.value = data
    // Initialize selections based on current status
    data.forEach(e => {
      selectedRsvps[e.id] = e.status === 'Attending'
    })
  } catch (e) {
    toast.add({ title: 'Error', description: 'Failed to load upcoming sessions.', color: 'red' })
  } finally {
    eventsLoading.value = false
  }
}

const submitBulkRsvp = async () => {
  submittingRsvp.value = true
  try {
    const selections = upcomingEvents.value.map(e => ({
      eventId: e.id,
      status: selectedRsvps[e.id] ? 'Attending' : 'Not Attending'
    }))

    await $fetch('/api/parent/actions/bulk-register', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: { selections }
    })

    toast.add({ title: 'Success', description: 'Attendance updated successfully.', color: 'green' })
    isRsvpModalOpen.value = false
    refreshDashboard()
  } catch (e) {
    toast.add({ title: 'Error', description: 'Could not update attendance.', color: 'red' })
  } finally {
    submittingRsvp.value = false
  }
}

// Change Password Logic
const isChangePasswordModalOpen = ref(false)
const changingPassword = ref(false)
const passwordForm = reactive({
  currentPassword: '',
  newPassword: '',
  confirmPassword: ''
})

const changePassword = () => {
  passwordForm.currentPassword = ''
  passwordForm.newPassword = ''
  passwordForm.confirmPassword = ''
  isChangePasswordModalOpen.value = true
}

const submitChangePassword = async () => {
  if (passwordForm.newPassword !== passwordForm.confirmPassword) {
    toast.add({ title: 'Error', description: 'Passwords do not match.', color: 'red' })
    return
  }

  changingPassword.value = true
  try {
    await $fetch('/api/auth/change-password', {
      method: 'POST',
      headers: getAuthHeaders(),
      body: {
        currentPassword: passwordForm.currentPassword,
        newPassword: passwordForm.newPassword
      }
    })
    toast.add({ title: 'Success', description: 'Password changed successfully.', color: 'green' })
    isChangePasswordModalOpen.value = false
  } catch (err: any) {
    toast.add({ title: 'Error', description: err?.data?.message || 'Failed to change password.', color: 'red' })
  } finally {
    changingPassword.value = false
  }
}
</script>
